using Microsoft.CodeAnalysis;
using PiggyRuntime;

namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;
    using System.Collections.Generic;
    using System.Collections;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System;
    using System.Reflection;

    public class NfaMatch
    {
        public class EnumerableIParseTree : IEnumerable<IParseTree>
        {
            IParseTree _start;
            public EnumerableIParseTree(IParseTree start)
            {
                _start = start;
            }

            private IEnumerator<IParseTree> Doit()
            {
                Stack<IParseTree> stack = new Stack<IParseTree>();
                HashSet<IParseTree> visited = new HashSet<IParseTree>();
                stack.Push(_start);
                while (stack.Count > 0)
                {
                    var v = stack.Pop();
                    if (visited.Contains(v))
                        yield return v;
                    else
                    {
                        stack.Push(v);
                        visited.Add(v);
                        for (int i = v.ChildCount - 1; i >= 0; --i)
                        {
                            var c = v.GetChild(i);
                            stack.Push(c);
                        }
                    }
                }
            }

            public IEnumerator<IParseTree> GetEnumerator()
            {
                return Doit();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Doit();
            }
        }

        private Dictionary<IParseTree, MethodInfo> _code_blocks;
        private object _instance;
        private Automaton nfa;

        public NfaMatch(Dictionary<IParseTree, MethodInfo> code_blocks,
            object instance,
            Automaton a)
        {
            nfa = a;
            _code_blocks = code_blocks;
            _instance = instance;
        }

        public string ReplaceMacro(IParseTree p)
        {
            // Try in order current type, then all other types.
            try
            {
                var res = _code_blocks[p].Invoke(_instance, new object[] { });
                return res as string;
            }
            catch (Exception e)
            {
            }
            throw new Exception("Cannot eval expression.");
        }

        public bool FindMatches(
            List<Path> currentPathList,
            List<State> currentStateList,
            ref List<Path> nextPathList,
            ref List<State> nextStateList,
            IParseTree input,
            int start = 0)
        {
            if (!(input as AstParserParser.NodeContext != null || input as AstParserParser.AttrContext != null))
                throw new Exception();

            int listID = 0;

            var generation = new Dictionary<Edge, int>();

            System.Console.Error.WriteLine("IN------");
            System.Console.Error.WriteLine("FindMatches "
                                           + input.GetText().Substring(
                                               0,
                                               input.GetText().Length > 50 ? 50 : input.GetText().Length));
            foreach(var path in currentPathList)
                System.Console.Error.WriteLine(path.ToString());
            System.Console.Error.WriteLine("IN------");

            // Variable "input" can be either one of two types:
            // AstParserParser.DeclContext
            // AstParserParser.AttrContext
            // Go through all children and match.
            int i = 0;
            for (;;)
            {
                var oldlist = currentPathList;
                var oldStateList = currentStateList;
                if (i >= input.ChildCount)
                {
                    nextPathList = currentPathList;
                    nextStateList = currentStateList;
                    break;
                }
                var c = input.GetChild(i);
                var t = c.GetText();

                i++;

                listID = Step(c, currentStateList, nextStateList, currentPathList, nextPathList, listID);
                if (!nextPathList.Any())
                    break;
                currentPathList = nextPathList;
                currentStateList = nextStateList;
                nextPathList = new List<Path>();
                nextStateList = new List<State>();
            }

            // Compute results.
            int matches = 0;
            for (int j = 0; j < nextPathList.Count; j++)
            {
                Path l = nextPathList[j];
                Edge e = l.LastEdge;
                State s = e._to;
                if (s.IsFinalState())
                {
                    matches++;
                }
            }
            foreach (var s in currentStateList)
            {
                if (s.IsFinalState())
                {
                    matches++;
                    if (!nextStateList.Contains(s))
                        nextStateList.Add(s);
                }
            }
            System.Console.Error.WriteLine("OUT------");
            System.Console.Error.WriteLine("FindMatches "
                                           + input.GetText().Substring(
                                               0,
                                               input.GetText().Length > 50 ? 50 : input.GetText().Length));
            foreach (var path in nextPathList)
                System.Console.Error.WriteLine(path.ToString());
            System.Console.Error.WriteLine("OUT------");
            return matches != 0;
        }

        public void AddStateAndClosure(List<State> list, State s)
        {
            if (list.Contains(s)) return;
            list.Add(s);
            foreach (var e in s.Owner.SuccessorEdges(s))
                if (e.IsEmpty || e.IsCode || e.IsText)
                    AddStateAndClosure(list, e._to);
        }

        void CheckPath(Path path)
        {
            return;
            State p = null;
            foreach (var ee in path)
            {
                var e = ee.LastEdge;
                if (p == null)
                    p = e._to;
                else
                {
                    if (p != e._from)
                        throw new System.Exception();
                    p = e._to;
                }
            }
        }

        private int Step(
            IParseTree input,
            List<State> currentStateList,
            List<State> nextStateList,
            List<Path> currentPathList,
            List<Path> nextPathList,
            int listID)
        {
            System.Console.Error.WriteLine("IN------");
            System.Console.Error.WriteLine("Step "
                                           + input.GetText().Substring(
                                               0,
                                               input.GetText().Length > 50 ? 50 : input.GetText().Length));
            foreach (var path in currentPathList)
                System.Console.Error.WriteLine(path.ToString());
            System.Console.Error.WriteLine("IN------");


            var t = input.GetText();
            var is_attr_or_node = input as AstParserParser.NodeContext != null ||
                                  input as AstParserParser.AttrContext != null;
            listID++;
            var ty = input.GetType();
            if (currentPathList == null || currentPathList.Count == 0)
            {
                for (int i = 0; i < currentStateList.Count; i++)
                {
                    State s = currentStateList[i];
                    foreach (Edge e in s.Owner.SuccessorEdges(s))
                    {
                        if (e._input == Edge.EmptyString)
                        {
                            AppendEdgeToPathSet(null, null, nextPathList, e, listID);
                        }
                        else if (e._input == input.GetText())
                        {
                            AppendEdgeToPathSet(input, null, nextPathList, e, listID);
                        }
                        else if (e._input == "<" && "(" == input.GetText())
                        {
                            AppendEdgeToPathSet(input, null, nextPathList, e, listID);
                        }
                        else if (e._input == ">" && ")" == input.GetText())
                        {
                            AppendEdgeToPathSet(input, null, nextPathList, e, listID);
                        }
                        else if (e.IsAny)
                        {
                            AppendEdgeToPathSet(input, null, nextPathList, e, listID);
                        }
                    }
                }
            }
            else
            {
                if (is_attr_or_node)
                {
                    for (int i = 0; i < currentPathList.Count; i++)
                    {
                        Path p = currentPathList[i];
                        CheckPath(p);
                        Edge l = p.LastEdge;
                        State s = l._to;
                        foreach (Edge e in s.Owner.SuccessorEdges(s))
                        {
                            if (e.IsAny)
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                        }
                    }
                    var cpl = currentPathList.ToList();
                    var csl = currentStateList.ToList();
                    var npl = new List<Path>();
                    var nsl = new List<State>();
                    this.FindMatches(currentPathList, currentStateList, ref npl, ref nsl, input);
                    foreach (var p in npl) nextPathList.Add(p);
                    foreach (var s in nsl) nextStateList.Add(s);
                }
                else
                {
                    for (int i = 0; i < currentPathList.Count; i++)
                    {
                        Path p = currentPathList[i];
                        CheckPath(p);
                        Edge l = p.LastEdge;
                        State s = l._to;
                        foreach (Edge e in s.Owner.SuccessorEdges(s))
                        {
                            if (e.IsAny)
                            {
                                // The "." transition only matches with attr or node
                                if (is_attr_or_node)
                                {
                                    AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                                }
                            }
                            else if (e._input == input.GetText())
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e._input == "<" && "(" == input.GetText())
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e._input == ">" && ")" == input.GetText())
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e._input == "*")
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e._input.StartsWith("$\""))
                            {
                                string pattern = e._input.Substring(2);
                                pattern = pattern.Substring(0, pattern.Length - 1);
                                try
                                {
                                    var ch = e._input;
                                    if (e.AstList.Count() > 1)
                                        ;
                                    //throw new Exception("Cannot compute interpolated pattern because there are multiple paths through the DFA with this edge.");
                                    IParseTree attr = e.AstList.First();
                                    pattern = ReplaceMacro(attr);
                                }
                                catch (System.Exception ex)
                                {
                                    System.Console.WriteLine("Cannot perform substitution in pattern with string.");
                                    System.Console.WriteLine("Pattern " + pattern);
                                    System.Console.WriteLine(ex.Message);
                                    throw ex;
                                }

                                pattern = pattern.Replace("\\", "\\\\");
                                Regex re = new Regex(pattern);
                                string tvaltext = input.GetText();
                                tvaltext = tvaltext.Substring(1);
                                tvaltext = tvaltext.Substring(0, tvaltext.Length - 1);
                                var matched = re.Match(tvaltext);
                                var result = matched.Success;
                                if (result)
                                    AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else
                            {
                            }
                        }
                    }
                }
            }
            foreach (var s in currentStateList)
            {
                if (s.IsFinalState())
                    AddStateAndClosure(nextStateList, s);
            }
            for (int i = 0; i < nextPathList.Count; i++)
			{
				Path p = nextPathList[i];
				Edge l = p.LastEdge;
				State s = l._to;
				AddStateAndClosure(nextStateList, s);
			}
            System.Console.Error.WriteLine("OUT------");
            System.Console.Error.WriteLine("Step "
                                           + input.GetText().Substring(
                                               0,
                                               input.GetText().Length > 50 ? 50 : input.GetText().Length));
            foreach (var path in nextPathList)
                System.Console.Error.WriteLine(path.ToString());
            System.Console.Error.WriteLine("OUT------");
            return listID;
        }

        private void AppendEdgeToPathSet(IParseTree c, Path path, List<Path> list, Edge e, int listID)
        {
            var st = e._to;
            var sf = e._from;
            foreach (var l in list)
            {
                CheckPath(l);
            }
            if (path == null && !list.Any())
            {
                list.Add(new Path(e, c));
            }
            else
            {
                if (e.IsAny)
                {
                }
                else
                {
                    if (path.Change != 0)
                        return;
                }
                var p = new Path(path, e, c);
                CheckPath(p);
                list.Add(p);
            }
            var added = list.Last();
            // If s contains any edges over epsilon, then add them.
            foreach (var o in st.Owner.SuccessorEdges(st))
                if (o.IsEmpty || o.IsCode || o.IsText)
                {
                    AppendEdgeToPathSet(null, added, list, o, listID);
                }
        }
    }
}
