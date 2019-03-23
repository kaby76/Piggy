namespace PiggyGenerator
{
    using System.Collections.Generic;
    using System;

    public class ClosureTaker
    {
        private ClosureTaker() { }
        public static MultiMap<string, Edge> GatherTransitions(IEnumerable<State> state_set)
        {
            MultiMap<string, Edge> transitions = new MultiMap<string, Edge>();
            foreach (var s in state_set)
            {
                foreach (var e in s._out_edges)
                {
                    if (!Automaton.IsLambdaTransition(e))
                    {
                        var str = e._c;
                        if (e.IsAny) str = "...";
                        else if (e.IsCode) str = "";
                        else if (e.IsText) str = "";
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
            for (int i = 0; i < list.Count; i++)
            {
                var state = list[i];
                var transitions = automaton.AllEdges(state);
                foreach (var transition in transitions)
                {
                    if (Automaton.IsLambdaTransition(transition))
                    {
                        var toState = transition._to;
                        if (list.Contains(toState)) continue;
                        list.Add(toState);
                    }
                }
            }
            var result = new SmartSet<State>();
            result.UnionWith(list);
            return result;
        }
    }
}
