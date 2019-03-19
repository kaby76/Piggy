namespace PiggyGenerator
{
    using System.Collections.Generic;

    public class ClosureTaker
    {
        private ClosureTaker() { }

        public static SmartSet<State> GetClosure(IEnumerable<State> states, Automaton automaton)
        {
            var list = new SmartSet<State>();
            foreach (var state in states) list.Add(state);
            bool changed = true;
            for (; ;)
            {
                changed = false;
                SmartSet<State> new_list = new SmartSet<State>();
                foreach (var state in list)
                {
                    var transitions = automaton.AllEdges(state);
                    foreach (var transition in transitions)
                    {
                        if (automaton.IsLambdaTransition(transition))
                        {
                            var toState = transition._to;
                            if (new_list.Contains(toState)) continue;
                            changed = true;
                            new_list.Add(toState);
                        }
                    }
                }
                if (!changed) break;
                list = new_list;
            }
            return list;
        }
    }
}
