namespace PiggyGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using Antlr4.Runtime.Tree;
    using System.Collections.Generic;
    using System.Text;
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
                var dfa_state = stack.Pop();
                var nfa_state_set = FindHashSet(dfa_state);
                var transitions = ClosureTaker.GatherTransitions(nfa_state_set);
                foreach (KeyValuePair<string, List<Edge>> transition_set in transitions)
                {
                    // Note, transitions is a collection of edges for a given string.
                    // For the NFA, an edge has one Ast for the edge because it came from one pattern.
                    // But, for the DFA, there could be multiple edges for the same string,
                    // each from a different pattern! Compute the set of Asts for
                    // all edges.
                    var key = transition_set.Key;
                    var value = transition_set.Value;
                    var state_set = new HashSet<State>();
                    foreach (var e in value) state_set.Add(e._to);
                    var new_state_set = ClosureTaker.GetClosure(state_set, nfa);
                    var new_dfa_state = FindHashSetState(dfa, new_state_set);
                    if (new_dfa_state == null)
                    {
                        State state = AddHashSetState(dfa, new_state_set);
                        state.Commit();
                        stack.Push(state);
                        new_dfa_state = state;
                        bool mark = false;
                        foreach (var s in new_state_set)
                            if (nfa.EndStates.Contains(s))
                                mark = true;
                        if (mark && !dfa.EndStates.Contains(new_dfa_state))
                            dfa.AddEndState(new_dfa_state);
                    }
                    // Add edges, if it doesn't exist already.
                    int mods = value.First()._edge_modifiers;
                    var asts = new List<IParseTree>();
                    foreach (Edge v in value)
                        foreach (IParseTree v2 in v.AstList)
                            asts.Add(v2);
                    var he = new Edge(dfa, dfa_state, new_dfa_state, asts, mods);
                    if (!new_dfa_state._out_edges.Contains(he)) he.Commit();
                }
            }
            return dfa;
        }
    }
}
