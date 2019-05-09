using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Tree;
using Campy.Graphs;

namespace PiggyGenerator
{
    public class NfaOptimize
    {
        private readonly Dictionary<State, SmartSet<State>> _closure = new Dictionary<State, SmartSet<State>>();
        private readonly Dictionary<SmartSet<State>, State> _hash_sets = new Dictionary<SmartSet<State>, State>();

        public State CreateInitialState(Automaton nfa, Automaton dfa)
        {
            /** get closure of initial state from nfa. */
            var initialStates = nfa.StartStates;
            var initialClosure = ClosureTaker.GetClosure(initialStates, nfa);
            var state = AddHashSetState(dfa, initialClosure);
            dfa.AddStartState(state);
            return state;
        }

        public bool HasFinalState(IEnumerable<State> states, Automaton automaton)
        {
            foreach (var state in states)
                if (automaton.IsFinalState(state))
                    return true;
            return false;
        }

        public State AddHashSetState(Automaton dfa, SmartSet<State> states)
        {
            var result = FindHashSetState(dfa, states);
            if (result != null) return result;
            result = new State(dfa);
            _hash_sets.Add(states, result);
            return result;
        }

        public State FindHashSetState(Automaton dfa, SmartSet<State> states)
        {
            State result = null;
            _hash_sets.TryGetValue(states, out result);
            return result;
        }

        public SmartSet<State> FindHashSet(State state)
        {
            foreach (var hs in _hash_sets)
                if (hs.Value == state)
                    return hs.Key;
            return null;
        }

        public Automaton ConvertToDFA(Automaton nfa)
        {
            var dfa = new Automaton();
            // For every state s, compute collection of states along epsilon edges
            // to get an initial computation of dfa states.
            foreach (var s in nfa.AllStates())
            {
                var c = ClosureTaker.GetClosure(new List<State> {s}, nfa);
                _closure[s] = c;
            }

            // For every state set, compute sums and fix up state sets.
            foreach (var p in _closure)
            {
                var key = p.Key;
                var set = p.Value;
                foreach (var s in set) _closure[s].UnionWith(set);
            }

            // For every state in nfa using Tarjan walk,
            // sum sets with common transitions.
            var ordered_list = new TarjanNoBackEdges<State, Edge>(nfa).ToList();
            ordered_list.Reverse();
            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var s in ordered_list)
                {
                    var closure = _closure[s];
                    var transitions = ClosureTaker.GatherTransitions(closure);
                    foreach (var transition_set in transitions)
                    {
                        var key = transition_set.Key;
                        var value = transition_set.Value;
                        var state_set = new SmartSet<State>();
                        // All states in value must have common set in dfa.
                        foreach (var e in value)
                        {
                            var c = e.To;
                            var cl = _closure[c];
                            state_set.UnionWith(cl);
                        }

                        foreach (var c in state_set)
                            if (!_closure[c].Equals(state_set))
                            {
                                _closure[c] = state_set;
                                changed = true;
                            }
                    }
                }
            }

            var initialState = CreateInitialState(nfa, dfa);
            foreach (var p in _closure)
            {
                var state_set = p.Value;
                var new_dfa_state = FindHashSetState(dfa, state_set);
                if (new_dfa_state == null)
                {
                    var state = AddHashSetState(dfa, state_set);
                    {
                        var mark = false;
                        foreach (var s in state_set)
                            if (nfa.FinalStates.Contains(s))
                                mark = true;
                        if (mark && !dfa.FinalStates.Contains(state))
                            dfa.AddFinalState(state);
                    }
                }
            }

            //System.Console.Error.WriteLine(dfa.ToString());

            foreach (var p in _closure)
            {
                var k = p.Key;
                var state_set = p.Value;
                var dfa_state = FindHashSetState(dfa, state_set);
                //     System.Console.Error.WriteLine("State " + dfa_state.Id + ":"
                //                                    + state_set.Aggregate(
                //                                        "", // start with empty string to handle empty list case.
                //                                       (current, next) => current + ", " + next));
            }

            //System.Console.Error.WriteLine(dfa.ToString());
            foreach (var from_dfa_state in dfa.AllStates())
            {
                var nfa_state_set = FindHashSet(from_dfa_state);
                var transitions = ClosureTaker.GatherTransitions(nfa_state_set);
                foreach (var transition_set in transitions)
                {
                    // Note, transitions is a collection of edges for a given string.
                    // For the NFA, an edge has one Ast for the edge because it came from one pattern.
                    // But, for the DFA, there could be multiple edges for the same string,
                    // each from a different pattern! Compute the set of Asts for
                    // all edges.
                    var key = transition_set.Key;
                    var value = transition_set.Value;
                    var state_set = new HashSet<State>();
                    foreach (var e in value) state_set.Add(e.To);
                    // Find in all previous states.
                    var new_state_set = _hash_sets.Where(hs => state_set.IsSubsetOf(hs.Key)).FirstOrDefault().Key;
                    if (new_state_set == null)
                        new_state_set = _closure[state_set.First()];
                    var to_dfa_state = FindHashSetState(dfa, new_state_set);
                    var mods = value.First().EdgeModifiers;
                    var asts = new List<IParseTree>();
                    foreach (var v in value)
                    foreach (var v2 in v.AstList)
                        asts.Add(v2);
                    var he = new Edge(dfa, from_dfa_state, to_dfa_state, asts, mods);
                }
            }

            // Add in "any" fragment in order to match tree nodes that aren't in pattern.
            //{
            //    State s3 = new State(dfa); s3.Commit();
            //    var e1 = new Edge(dfa, s3, s3, null, Edge.EmptyAst, (int)Edge.EdgeModifiers.Any); e1.Commit();
            //    var f = new Fragment(s3);
            //}

            return dfa;
        }
    }
}