
namespace PiggyGenerator
{
    using System.Collections;
    using System.Collections.Generic;
    using Antlr4.Runtime.Tree;
    using System.Linq;
    using System;
    using System.Text.RegularExpressions;

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
        private TreeRegEx _tree_re;

        public NfaMatch(TreeRegEx tree_re)
        {
            MatchingPaths = new List<Path>();
            _tree_re = tree_re;
        }

        public bool IsMatch(Automaton nfa, IParseTree input)
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
                    listID = stepPath(start_state_list, c, nextList, listID, generation);
                    first = false;
                }
                else
                {
                    listID = stepPath(currentList, c, nextList, listID, generation);
                }
                currentList = nextList;
                if (!currentList.Any())
                    break;
                nextList = new List<Path>();
            }
            var result = ContainsMatchState(currentList);
            return result;
        }

        private bool ContainsMatchState(List<Path> finalList)
        {
            int matches = 0;
            for (int i = 0; i < finalList.Count; i++)
            {
                Path l = finalList[i];
                //foreach (var x in l)
                //    System.Console.WriteLine(x);
                Edge e = l.LastEdge;
                State s = e._to;
                if (s.isMatch())
                {
                    matches++;
                    MatchingPaths.Add(l);
                }
            }
            return matches > 0;
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

        private int stepPath(List<Path> currentList, IParseTree c, List<Path> nextList, int listID, Dictionary<State, int> gen)
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
                    if (e._c_text == null)
                    {
                        addPath(null, p, nextList, e, listID, gen);
                    }
                    else if (e._c_text == c.GetText())
                    {
                        addPath(c, p, nextList, e, listID, gen);
                    }
                    else if (e._c_text == "<" && "(" == c.GetText())
                    {
                        addPath(c, p, nextList, e, listID, gen);
                    }
                    else if (e._c_text == ">" && ")" == c.GetText())
                    {
                        addPath(c, p, nextList, e, listID, gen);
                    }
                    else if (e._c_text == "*")
                    {
                        addPath(c, p, nextList, e, listID, gen);
                    }
                    else if (e._any)
                    {
                        addPath(c, p, nextList, e, listID, gen);
                    }
                    else if (e._c_text.StartsWith("$\""))
                    {
                        string pattern = e._c_text.Substring(2);
                        pattern = pattern.Substring(0, pattern.Length - 1);
                        try
                        {
                            var attr = e._c;
                            for (; ; )
                            {
                                if (attr == null) break;
                                if (attr as SpecParserParser.AttrContext != null) break;
                                attr = this._tree_re._parent[attr];
                            }
                            pattern = _tree_re.ReplaceMacro(attr);
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
                            addPath(c, p, nextList, e, listID, gen);
                    }
                    else
                    {
                    }
                }
            }
            return listID;
        }

        private int stepPath(List<State> currentList, IParseTree c, List<Path> nextList, int listID, Dictionary<State, int> gen)
        {
            listID++;
            for (int i = 0; i < currentList.Count; i++)
            {
                State s = currentList[i];
                foreach (Edge e in s._out_edges)
                {
                    if (e._c_text == null)
                    {
                        addPath(null, null, nextList, e, listID, gen);
                    }
                    else if (e._c_text == c.GetText())
                    {
                        addPath(c, null, nextList, e, listID, gen);
                    }
                    else if (e._c_text == "<" && "(" == c.GetText())
                    {
                        addPath(c, null, nextList, e, listID, gen);
                    }
                    else if (e._c_text == ">" && ")" == c.GetText())
                    {
                        addPath(c, null, nextList, e, listID, gen);
                    }
                    else if (e._any)
                    {
                        addPath(c, null, nextList, e, listID, gen);
                    }
                }
            }
            return listID;
        }

        private void addPath(IParseTree c, Path path, List<Path> list, Edge e, int listID, Dictionary<State, int> gen)
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
                if (o._c == null)
                {
                    addPath(null, added, list, o, listID, gen);
                }
        }
    }
}
