namespace PiggyGenerator
{
    using System.Collections.Generic;

    public class ClosureTaker
    {
        private ClosureTaker() { }

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
