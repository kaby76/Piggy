
namespace PiggyGenerator
{
    using System.Collections;
    using System.Collections.Generic;
    using Antlr4.Runtime.Tree;
    using System.Linq;

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

        /**
         * The simulateNFA method contains the algorithm used for simulating an NFA. The algorithm used is the one introduced by
         * Ken Thompson in his 1968 paper concerning regular expression checking, the most important feature of which is multiple
         * state simulation, meaning that the algorithm keeps a list of all possible states of the automata and advances all these
         * possibilities in one step, instead of recursively trying each of the possible paths of the automata. So, for each character
         * in the input string, the algorithm tries to advance onwards from all the possible states it can be. When a single
         * advance in the automata has a time-requirement of O(1), this gives us a total maximum time requirement of O(nm), where n
         * is the length of the string that is being checked, and m is the number of states in the NFA (which corresponds to
         * the length of the regular expression used to construct the NFA).
         * @param startstate the starting state of the NFA
         * @param input the string that is to be checked against the NFA
         * @return true, if the string given as input matches the regular expression that the NFA represents, otherwise false.
         */
        public bool IsMatchOld(NFA nfa, IParseTree input)
        {
            List<State> currentList = new List<State>();
            List<State> nextList = new List<State>();
            var generation = new Dictionary<State, int>();
            int listID = 0;
            var start_state = nfa._start_state;
            addState(currentList, start_state, listID, generation);
            foreach (var c in new EnumerableIParseTree(input))
            {
                if (c as TerminalNodeImpl == null)
                    continue;
                listID = step(currentList, c, nextList, listID, generation);
                currentList = nextList;
                if (!currentList.Any())
                    break;
                nextList = new List<State>();
            }
            var result = containsMatchState(currentList);
            return result;
        }

        public bool IsMatch(NFA nfa, IParseTree input)
        {
            var currentList = new List<List<Edge>>();
            var nextList = new List<List<Edge>>();
            var generation = new Dictionary<State, int>();
            int listID = 0;
            bool first = true;
            foreach (var c in new EnumerableIParseTree(input))
            {
                if (c as TerminalNodeImpl == null)
                    continue;
                if (first)
                {
                    var start_state = nfa._start_state;
                    var start_state_list = new List<State>();
                    addState(start_state_list, start_state, listID, generation);
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
                nextList = new List<List<Edge>>();
            }
            var result = containsMatchState(currentList);
            return result;
        }

        private bool containsMatchState(List<State> finalList)
        {
            for (int i = 0; i < finalList.Count; i++)
            {
                State s = finalList[i];
                if (s.isMatch())
                {
                    return true;
                }
            }
            return false;
        }

        private bool containsMatchState(List<List<Edge>> finalList)
        {
            int matches = 0;
            for (int i = 0; i < finalList.Count; i++)
            {
                List<Edge> l = finalList[i];
                //foreach (var x in l)
                //    System.Console.WriteLine(x);
                Edge e = l.Last();
                State s = e._to;
                if (s.isMatch())
                {
                    matches++;
                }
            }
            return matches > 0;
        }

        private int step(List<State> currentList, IParseTree c, List<State> nextList, int listID, Dictionary<State, int> gen)
        {
            listID++;
            for (int i = 0; i < currentList.Count; i++)
            {
                State s = currentList[i];
                foreach (Edge e in s._out_edges)
                {
                    if (e._c_text == null)
                    {
                        addState(nextList, e._to, listID, gen);
                    }
                    else if (e._c_text == c.GetText())
                    {
                        addState(nextList, e._to, listID, gen);
                    }
                    else if (e._c_text == "<" && "(" == c.GetText())
                    {
                        addState(nextList, e._to, listID, gen);
                    }
                    else if (e._c_text == ">" && ")" == c.GetText())
                    {
                        addState(nextList, e._to, listID, gen);
                    }
                    else if (e._any)
                    {
                        addState(nextList, e._to, listID, gen);
                    }
                }
            }
            return listID;
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

        void CheckPath(List<Edge> path)
        {
            State p = null;
            foreach (var e in path)
            {
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

        private int stepPath(List<List<Edge>> currentList, IParseTree c, List<List<Edge>> nextList, int listID, Dictionary<State, int> gen)
        {
            listID++;
            for (int i = 0; i < currentList.Count; i++)
            {
                List<Edge> p = currentList[i];
                CheckPath(p);
                Edge l = p.Last();
                State s = l._to;
                foreach (Edge e in s._out_edges)
                {
                    if (e._c_text == null)
                    {
                        addPath(p, nextList, e, listID, gen);
                    }
                    else if (e._c_text == c.GetText())
                    {
                        addPath(p, nextList, e, listID, gen);
                    }
                    else if (e._c_text == "<" && "(" == c.GetText())
                    {
                        addPath(p, nextList, e, listID, gen);
                    }
                    else if (e._c_text == ">" && ")" == c.GetText())
                    {
                        addPath(p, nextList, e, listID, gen);
                    }
                    else if (e._any)
                    {
                        addPath(p, nextList, e, listID, gen);
                    }
                }
            }
            return listID;
        }

        private int stepPath(List<State> currentList, IParseTree c, List<List<Edge>> nextList, int listID, Dictionary<State, int> gen)
        {
            listID++;
            for (int i = 0; i < currentList.Count; i++)
            {
                State s = currentList[i];
                foreach (Edge e in s._out_edges)
                {
                    if (e._c_text == null)
                    {
                        addPath(null, nextList, e, listID, gen);
                    }
                    else if (e._c_text == c.GetText())
                    {
                        addPath(null, nextList, e, listID, gen);
                    }
                    else if (e._c_text == "<" && "(" == c.GetText())
                    {
                        addPath(null, nextList, e, listID, gen);
                    }
                    else if (e._c_text == ">" && ")" == c.GetText())
                    {
                        addPath(null, nextList, e, listID, gen);
                    }
                    else if (e._any)
                    {
                        addPath(null, nextList, e, listID, gen);
                    }
                }
            }
            return listID;
        }

        private void addPath(List<Edge> path, List<List<Edge>> list, Edge e, int listID, Dictionary<State, int> gen)
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
                list.Add(new List<Edge>() { e });
            }
            else
            {
                var copy = path.ToList();
                copy.Add(e);
                CheckPath(copy);
                list.Add(copy);
            }
            var added = list.Last();
            // If s contains any edges over epsilon, then add them.
            foreach (var o in s._out_edges)
                if (o._c == null)
                {
                    addPath(added, list, o, listID, gen);
                }
        }
    }
}
