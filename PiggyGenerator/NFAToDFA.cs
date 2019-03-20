﻿namespace PiggyGenerator
{
    using System.Collections.Generic;
    using System.Linq;

    public class NFAToDFA
    {
        Dictionary<SmartSet<State>, State> _hash_sets = new Dictionary<SmartSet<State>, State>();

        public NFAToDFA()
        {
        }

        public State CreateInitialState(Automaton nfa, Automaton dfa)
        {
            /** get closure of initial state from nfa. */
            var initialStates = nfa.StartStates;
            var initialClosure = ClosureTaker.GetClosure(initialStates, nfa);
            State state = AddHashSetState(dfa, initialClosure);
            dfa.AddStartState(state);
            return state;
        }

        public bool HasFinalState(IEnumerable<State> states, Automaton automaton)
        {
            foreach (var state in states)
            {
                if (automaton.IsFinalState(state))
                    return true;
            }
            return false;
        }

        public State AddHashSetState(Automaton dfa, SmartSet<State> states)
        {
            State result = FindHashSetState(dfa, states);
            if (result != null) return result;
            result = new State(dfa); result.Commit();
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
                if (hs.Value == state) return hs.Key;
            return null;
        }

        public Automaton ConvertToDFA(Automaton nfa)
        {
            var dfa = new Automaton();
            State initialState = CreateInitialState(nfa, dfa);
            Stack<State> stack = new Stack<State>();
            stack.Push(initialState);
            while (stack.Count > 0)
            {
                var from = stack.Pop();
                var nfa_state_set = FindHashSet(from);
                foreach (var nfa_state in nfa_state_set)
                {
                    if (nfa.EndStates.Contains(nfa_state) && !dfa.EndStates.Contains(from))
                        dfa.AddEndState(from);
                    foreach (var e in nfa_state._out_edges)
                    {
                        if (!Automaton.IsLambdaTransition(e))
                        {
                            var c = ClosureTaker.GetClosure(new List<State>() { e._to }, nfa);
                            var to = FindHashSetState(dfa, c);
                            if (to == null)
                            {
                                State state = AddHashSetState(dfa, c);
                                state.Commit();
                                stack.Push(state);
                                to = state;
                            }
                            // Add edges, if it doesn't exist already.
                            var he = new Edge(dfa, from, to, e._c, e._other, e._edge_modifiers);
                            if (!from._out_edges.Contains(he)) he.Commit();
                        }
                    }
                }
            }
            return dfa;
        }
    }
}