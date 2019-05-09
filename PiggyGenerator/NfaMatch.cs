using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Tree;
using PiggyRuntime;

namespace PiggyGenerator
{
    public class NfaMatch
    {
        private readonly Dictionary<IParseTree, MethodInfo> _code_blocks;
        private readonly object _instance;
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

            var listID = 0;

            var generation = new Dictionary<Edge, int>();

            Console.Error.WriteLine("IN------");
            Console.Error.WriteLine("FindMatches "
                                    + input.GetText().Substring(
                                        0,
                                        input.GetText().Length > 50 ? 50 : input.GetText().Length));
            foreach (var path in currentPathList)
                Console.Error.WriteLine(path.ToString());
            Console.Error.WriteLine("IN------");

            // Variable "input" can be either one of two types:
            // AstParserParser.DeclContext
            // AstParserParser.AttrContext
            // Go through all children and match.
            var i = 0;
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
            var matches = 0;
            for (var j = 0; j < nextPathList.Count; j++)
            {
                var l = nextPathList[j];
                var e = l.LastEdge;
                var s = e.To;
                if (s.Owner.IsFinalState(s)) matches++;
            }

            foreach (var s in currentStateList)
                if (s.Owner.IsFinalState(s))
                {
                    matches++;
                    if (!nextStateList.Contains(s))
                        nextStateList.Add(s);
                }

            Console.Error.WriteLine("OUT------");
            Console.Error.WriteLine("FindMatches "
                                    + input.GetText().Substring(
                                        0,
                                        input.GetText().Length > 50 ? 50 : input.GetText().Length));
            foreach (var path in nextPathList)
                Console.Error.WriteLine(path.ToString());
            Console.Error.WriteLine("OUT------");
            return matches != 0;
        }

        public void AddStateAndClosure(List<State> list, State s)
        {
            if (list.Contains(s)) return;
            list.Add(s);
            foreach (var e in s.Owner.SuccessorEdges(s))
                if (e.IsEmpty || e.IsCode || e.IsText)
                    AddStateAndClosure(list, e.To);
        }

        private void CheckPath(Path path)
        {
            return;
            State p = null;
            foreach (var ee in path)
            {
                var e = ee.LastEdge;
                if (p == null) p = e.To;

                if (p != e.From)
                    throw new Exception();
                p = e.To;
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
            Console.Error.WriteLine("IN------");
            Console.Error.WriteLine("Step "
                                    + input.GetText().Substring(
                                        0,
                                        input.GetText().Length > 50 ? 50 : input.GetText().Length));
            foreach (var path in currentPathList)
                Console.Error.WriteLine(path.ToString());
            Console.Error.WriteLine("IN------");


            var t = input.GetText();
            var is_attr_or_node = input as AstParserParser.NodeContext != null ||
                                  input as AstParserParser.AttrContext != null;
            listID++;
            var ty = input.GetType();
            if (currentPathList == null || currentPathList.Count == 0)
            {
                for (var i = 0; i < currentStateList.Count; i++)
                {
                    var s = currentStateList[i];
                    foreach (var e in s.Owner.SuccessorEdges(s))
                        if (e.Input == Edge.EmptyString)
                            AppendEdgeToPathSet(null, null, nextPathList, e, listID);
                        else if (e.Input == input.GetText())
                            AppendEdgeToPathSet(input, null, nextPathList, e, listID);
                        else if (e.Input == "<" && "(" == input.GetText())
                            AppendEdgeToPathSet(input, null, nextPathList, e, listID);
                        else if (e.Input == ">" && ")" == input.GetText())
                            AppendEdgeToPathSet(input, null, nextPathList, e, listID);
                        else if (e.IsAny) AppendEdgeToPathSet(input, null, nextPathList, e, listID);
                }
            }
            else
            {
                if (is_attr_or_node)
                {
                    for (var i = 0; i < currentPathList.Count; i++)
                    {
                        var p = currentPathList[i];
                        CheckPath(p);
                        var l = p.LastEdge;
                        var s = l.To;
                        foreach (var e in s.Owner.SuccessorEdges(s))
                            if (e.IsAny)
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                    }

                    var cpl = currentPathList.ToList();
                    var csl = currentStateList.ToList();
                    var npl = new List<Path>();
                    var nsl = new List<State>();
                    FindMatches(currentPathList, currentStateList, ref npl, ref nsl, input);
                    foreach (var p in npl) nextPathList.Add(p);
                    foreach (var s in nsl) nextStateList.Add(s);
                }
                else
                {
                    for (var i = 0; i < currentPathList.Count; i++)
                    {
                        var p = currentPathList[i];
                        CheckPath(p);
                        var l = p.LastEdge;
                        var s = l.To;
                        foreach (var e in s.Owner.SuccessorEdges(s))
                            if (e.IsAny)
                            {
                                // The "." transition only matches with attr or node
                                if (is_attr_or_node) AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e.Input == input.GetText())
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e.Input == "<" && "(" == input.GetText())
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e.Input == ">" && ")" == input.GetText())
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e.Input == "*")
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e.Input.StartsWith("$\""))
                            {
                                var pattern = e.Input.Substring(2);
                                pattern = pattern.Substring(0, pattern.Length - 1);
                                try
                                {
                                    var ch = e.Input;
                                    if (e.AstList.Count() > 1)
                                        ;
                                    //throw new Exception("Cannot compute interpolated pattern because there are multiple paths through the DFA with this edge.");
                                    var attr = e.AstList.First();
                                    pattern = ReplaceMacro(attr);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Cannot perform substitution in pattern with string.");
                                    Console.WriteLine("Pattern " + pattern);
                                    Console.WriteLine(ex.Message);
                                    throw ex;
                                }

                                pattern = pattern.Replace("\\", "\\\\");
                                var re = new Regex(pattern);
                                var tvaltext = input.GetText();
                                tvaltext = tvaltext.Substring(1);
                                tvaltext = tvaltext.Substring(0, tvaltext.Length - 1);
                                var matched = re.Match(tvaltext);
                                var result = matched.Success;
                                if (result)
                                    AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                    }
                }
            }

            foreach (var s in currentStateList)
                if (s.Owner.IsFinalState(s))
                    AddStateAndClosure(nextStateList, s);

            for (var i = 0; i < nextPathList.Count; i++)
            {
                var p = nextPathList[i];
                var l = p.LastEdge;
                var s = l.To;
                AddStateAndClosure(nextStateList, s);
            }

            Console.Error.WriteLine("OUT------");
            Console.Error.WriteLine("Step "
                                    + input.GetText().Substring(
                                        0,
                                        input.GetText().Length > 50 ? 50 : input.GetText().Length));
            foreach (var path in nextPathList)
                Console.Error.WriteLine(path.ToString());
            Console.Error.WriteLine("OUT------");
            return listID;
        }

        private void AppendEdgeToPathSet(IParseTree c, Path path, List<Path> list, Edge e, int listID)
        {
            var st = e.To;
            var sf = e.From;
            foreach (var l in list) CheckPath(l);

            if (path == null && !list.Any())
            {
                list.Add(new Path(e, c));
            }
            else
            {
                var p = new Path(path, e, c);
                CheckPath(p);
                list.Add(p);
            }

            var added = list.Last();
            // If s contains any edges over epsilon, then add them.
            foreach (var o in st.Owner.SuccessorEdges(st))
                if (o.IsEmpty || o.IsCode || o.IsText)
                    AppendEdgeToPathSet(null, added, list, o, listID);
        }

        public class EnumerableIParseTree : IEnumerable<IParseTree>
        {
            private readonly IParseTree _start;

            public EnumerableIParseTree(IParseTree start)
            {
                _start = start;
            }

            public IEnumerator<IParseTree> GetEnumerator()
            {
                return Doit();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Doit();
            }

            private IEnumerator<IParseTree> Doit()
            {
                var stack = new Stack<IParseTree>();
                var visited = new HashSet<IParseTree>();
                stack.Push(_start);
                while (stack.Count > 0)
                {
                    var v = stack.Pop();
                    if (visited.Contains(v))
                    {
                        yield return v;
                    }
                    else
                    {
                        stack.Push(v);
                        visited.Add(v);
                        for (var i = v.ChildCount - 1; i >= 0; --i)
                        {
                            var c = v.GetChild(i);
                            stack.Push(c);
                        }
                    }
                }
            }
        }
    }
}