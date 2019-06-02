using System;
using System.Collections.Generic;

namespace Engine
{
    public class ClosureTaker
    {
        private ClosureTaker()
        {
        }

        public static MultiMap<string, Edge> GatherTransitions(IEnumerable<State> state_set)
        {
            var transitions = new MultiMap<string, Edge>();
            foreach (var s in state_set)
            {
                foreach (var e in s.Owner.SuccessorEdges(s))
                {
                    if (!Automaton.IsEpsilonTransition(e))
                    {
                        var str = e.Input;
                        if (e.IsAny) str = "...";
                        else if (e.IsCode) str = "";
                        else if (e.IsText) str = "";
                        else if (e.IsNot) str = "!" + str;
                        else if (str == null) throw new Exception();
                        transitions.Add(str, e);
                    }
                }
            }
            return transitions;
        }

        public static SmartSet<State> GetClosure(IEnumerable<State> states, Automaton automaton)
        {
            var list = new List<State>();
            foreach (var state in states) list.Add(state);
            for (var i = 0; i < list.Count; i++)
            {
                var state = list[i];
                var transitions = automaton.SuccessorEdges(state);
                foreach (var transition in transitions)
                    if (Automaton.IsEpsilonTransition(transition))
                    {
                        var toState = transition.To;
                        if (list.Contains(toState)) continue;
                        list.Add(toState);
                    }
            }

            var result = new SmartSet<State>();
            result.UnionWith(list);
            return result;
        }
    }
}