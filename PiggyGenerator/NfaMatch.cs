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

        private Dictionary<IParseTree, IParseTree> _parent;
        private Dictionary<IParseTree, MethodInfo> _code_blocks;
        private object _instance;

        public NfaMatch(Dictionary<IParseTree, IParseTree> parent,
            Dictionary<IParseTree, MethodInfo> code_blocks,
            object instance)
        {
            _parent = parent;
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

        public bool FindMatches(List<Path> MatchingPaths, Automaton nfa, IParseTree input, int start = 0)
        {
            if (start == 0)
            {
                start = nfa.StartStates.FirstOrDefault().Id;
            }

            var currentList = new List<Path>();
            var nextList = new List<Path>();
            var generation = new Dictionary<Edge, int>();
            int listID = 0;
            bool first = true;
            // Variable "input" can be either one of two types:
            // AstParserParser.DeclContext
            // AstParserParser.AttrContext
            // Go through all children and match.
            if (input as AstParserParser.NodeContext != null)
            {
                for (int i = 0; i < input.ChildCount; ++i)
                {
                    var c = input.GetChild(i);
                    var t = c.GetText();
                    if (i == 0)
                    {
                        var start_state_list = new List<State>();
                        var st = nfa.AllStates().Where(s => s.Id == start).FirstOrDefault();
                        addState(start_state_list, st, listID, generation);
                        listID = Step(start_state_list, c, nextList, listID, generation);
                    }
                    else
                    {
                        listID = Step(nfa, currentList, c, nextList, listID, generation);
                    }
                    var oldlist = currentList;
                    currentList = nextList;
                    if (!currentList.Any())
                        break;
                    nextList = new List<Path>();
                }
            }
            else if (input as AstParserParser.AttrContext != null)
            {
                for (int i = 0; i < input.ChildCount; ++i)
                {
                    var c = input.GetChild(i);
                    var t = c.GetText();
                    if (i == 0)
                    {
                        var start_state_list = new List<State>();
                        var st = nfa.AllStates().Where(s => s.Id == start).FirstOrDefault();
                        addState(start_state_list, st, listID, generation);
                        listID = Step(start_state_list, c, nextList, listID, generation);
                    }
                    else
                    {
                        listID = Step(nfa, currentList, c, nextList, listID, generation);
                    }
                    var oldlist = currentList;
                    currentList = nextList;
                    if (!currentList.Any())
                        break;
                    nextList = new List<Path>();
                }
            }
            else
            {
                return false;
            }
            int matches = 0;
            for (int i = 0; i < currentList.Count; i++)
            {
                Path l = currentList[i];
                //foreach (var x in l)
                //    System.Console.WriteLine(x);
                Edge e = l.LastEdge;
                State s = e._to;
                if (s.IsFinalState())
                {
                    matches++;
                    MatchingPaths.Add(l);
                }
                else if (start != 0 && s.IsFinalStateSubpattern())
                {
                    matches++;
                    MatchingPaths.Add(l);
                }
            }
            if (matches > 1)
            {
            }
            foreach (var m in MatchingPaths)
            {
                System.Console.Error.WriteLine("match ----- ");
                foreach (var ss in m)
                {
                    System.Console.Error.WriteLine(ss.LastEdge + " sym " + (ss.Ast == null ? "empty" : ss.Ast.GetText()));
                }
            }
            if (matches > 1)
            {
                throw new Exception("QUIT");
            }
            return matches != 0;
        }

        private void addState(List<State> list, State s, int listID, Dictionary<Edge, int> gen)
        {
            list.Add(s);
            // If s contains any edges over epsilon, then add them.
            foreach (var e in s._out_edges)
                if (e._c == null)
                    addState(list, e._to, listID, gen);
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

        private int Step(Automaton nfa, List<Path> currentList, IParseTree c, List<Path> nextList, int listID, Dictionary<Edge, int> gen)
        {
            listID++;
            for (int i = 0; i < currentList.Count; i++)
            {
                Path p = currentList[i];
                CheckPath(p);
                Edge l = p.LastEdge;
                State s = l._to;
                foreach (Edge e in s._out_edges)
                {
                    if (e.IsSubpattern)
                    {
                        // Set up to match c on subpattern. If no match,
                        // create skipping path.
                        var more = new List<Path>();
                        bool matched = this.FindMatches(more, nfa, c, e._fragment_start.Id);
                        if (matched)
                        {
                            foreach (Path ll in more)
                            {
                                var p2 = p;
                                foreach (Path xxx in ll)
                                {
                                    p2 = new Path(p2, xxx.LastEdge, xxx.Ast);                                    
                                }
                                // Set up state where we left off.
                                p2 = new Path(p2, new Edge(nfa, p2.LastEdge._to, e._to, Edge.EmptyAst), null);
                                nextList.Add(p2);
                            }
                        }
                        else
                        {
                            var any = nfa.AllStates().Last();
                            var any_edge = any._out_edges.First();
                            // Add in all of c into "any" path.
                            var p2 = p;
                            foreach (IParseTree zzz in new EnumerableIParseTree(c))
                            {
                                if (zzz as TerminalNodeImpl == null) continue;
                                p2 = new Path(p2, any_edge, zzz);
                            }
                            // Set up state where we left off.
                            p2 = new Path(p2, new Edge(nfa, any, e._from, Edge.EmptyAst), null);
                            nextList.Add(p2);
                        }
                    }
                    else if (e.IsAny)
                    {
                        AppendEdgeToPathSet(c, p, nextList, e, listID, gen);
                    }
                    else if (e._c == c.GetText())
                    {
                        AppendEdgeToPathSet(c, p, nextList, e, listID, gen);
                    }
                    else if (e._c == "<" && "(" == c.GetText())
                    {
                        AppendEdgeToPathSet(c, p, nextList, e, listID, gen);
                    }
                    else if (e._c == ">" && ")" == c.GetText())
                    {
                        AppendEdgeToPathSet(c, p, nextList, e, listID, gen);
                    }
                    else if (e._c == "*")
                    {
                        AppendEdgeToPathSet(c, p, nextList, e, listID, gen);
                    }
                    else if (e._c.StartsWith("$\""))
                    {
                        string pattern = e._c.Substring(2);
                        pattern = pattern.Substring(0, pattern.Length - 1);
                        try
                        {
                            var ch = e._c;
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
                        string tvaltext = c.GetText();
                        tvaltext = tvaltext.Substring(1);
                        tvaltext = tvaltext.Substring(0, tvaltext.Length - 1);
                        var matched = re.Match(tvaltext);
                        var result = matched.Success;
                        if (result)
                            AppendEdgeToPathSet(c, p, nextList, e, listID, gen);
                    }
                    else
                    {
                    }
                }
            }
            return listID;
        }

        private int Step(List<State> currentList, IParseTree c, List<Path> nextList, int listID, Dictionary<Edge, int> gen)
        {
            listID++;
            for (int i = 0; i < currentList.Count; i++)
            {
                State s = currentList[i];
                foreach (Edge e in s._out_edges)
                {
                    if (e._c == Edge.EmptyString)
                    {
                        AppendEdgeToPathSet(null, null, nextList, e, listID, gen);
                    }
                    else if (e._c == c.GetText())
                    {
                        AppendEdgeToPathSet(c, null, nextList, e, listID, gen);
                    }
                    else if (e._c == "<" && "(" == c.GetText())
                    {
                        AppendEdgeToPathSet(c, null, nextList, e, listID, gen);
                    }
                    else if (e._c == ">" && ")" == c.GetText())
                    {
                        AppendEdgeToPathSet(c, null, nextList, e, listID, gen);
                    }
                    else if (e.IsAny)
                    {
                        AppendEdgeToPathSet(c, null, nextList, e, listID, gen);
                    }
                }
            }
            return listID;
        }

        private void AppendEdgeToPathSet(IParseTree c, Path path, List<Path> list, Edge e, int listID, Dictionary<Edge, int> gen)
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
            foreach (var o in st._out_edges)
                if (o.IsEmpty || o.IsCode || o.IsText)
                {
                    AppendEdgeToPathSet(null, added, list, o, listID, gen);
                }
        }
    }
}
