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

        public List<Path> MatchingPaths
        {
            get; private set;
        }
        private Dictionary<IParseTree, IParseTree> _parent;
        private Dictionary<IParseTree, MethodInfo> _code_blocks;
        private object _instance;

        public NfaMatch(Dictionary<IParseTree, IParseTree> parent,
            Dictionary<IParseTree, MethodInfo> code_blocks,
            object instance)
        {
            MatchingPaths = new List<Path>();
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

        public bool FindMatches(Automaton nfa, IParseTree input)
        {
            var currentList = new List<Path>();
            var nextList = new List<Path>();
            var generation = new Dictionary<State, int>();
            int listID = 0;
            bool first = true;
            foreach (var c in new EnumerableIParseTree(input))
            {
                if (c as TerminalNodeImpl == null)
                    continue;
                if (first)
                {
                    var start_states = nfa.StartStates;
                    var start_state_list = new List<State>();
                    foreach (var s in start_states) addState(start_state_list, s, listID, generation);
                    listID = Step(start_state_list, c, nextList, listID, generation);
                    first = false;
                }
                else
                {
                    listID = Step(currentList, c, nextList, listID, generation);
                }
                currentList = nextList;
                if (!currentList.Any())
                    break;
                nextList = new List<Path>();
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
            }
            if (matches > 1)
            {
            }
            return matches != 0;
        }

        private void addState(List<State> list, State s, int listID, Dictionary<State, int> gen)
        {
            if (s == null || (gen.TryGetValue(s, out int last) && last == listID))
                return;
            gen[s] = listID;
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

        private int Step(List<Path> currentList, IParseTree c, List<Path> nextList, int listID, Dictionary<State, int> gen)
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
                    if (e._c == Edge.EmptyString)
                    {
                        AppendEdgeToPathSet(null, p, nextList, e, listID, gen);
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
                    else if (e.IsAny)
                    {
                        AppendEdgeToPathSet(c, p, nextList, e, listID, gen);
                    }
                    else if (e.IsCode || e.IsText)
                    {
                        AppendEdgeToPathSet(null, p, nextList, e, listID, gen);
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
                            for (; ; )
                            {
                                if (attr == null) break;
                                if (attr as SpecParserParser.AttrContext != null) break;
                                attr = _parent[attr];
                            }
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

        private int Step(List<State> currentList, IParseTree c, List<Path> nextList, int listID, Dictionary<State, int> gen)
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

        private void AppendEdgeToPathSet(IParseTree c, Path path, List<Path> list, Edge e, int listID, Dictionary<State, int> gen)
        {
            var s = e._to;
            var sf = e._from;
            foreach (var l in list)
            {
                CheckPath(l);
            }

            if (gen.TryGetValue(s, out int last) && last == listID)
                return;
            gen[s] = listID;
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
            foreach (var o in s._out_edges)
                if (Automaton.IsLambdaTransition(o))
                {
                    AppendEdgeToPathSet(null, added, list, o, listID, gen);
                }
        }
    }
}
