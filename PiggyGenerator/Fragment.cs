using System.Collections.Generic;
using System.Text;

namespace PiggyGenerator
{
    /**
     * The Fragment class specifies the Fragment object, that is used in the generation of an NFA. The Fragment is
     * a partial NFA, and by patching these Fragments together the algorithm specified in the NFA-class generates
     * the automata. The Fragment keeps track of the starting state of the partial automata, as well as the states with
     * "dangling arrows", that is, state transitions that do not point to anything yet.
     * @author Kimmo Heikkinen
     */
    public class Fragment
    {
        public Fragment(State start)
        {
            OutStates = new List<State>();
            OutStates.Add(start); // start state is also and exit state.
            StartState = start;
        }

        public Fragment(State start, State @out)
        {
            OutStates = new List<State>();
            OutStates.Add(@out);
            StartState = start;
        }

        public Fragment(State start, List<State> out_states)
        {
            OutStates = out_states;
            StartState = start;
        }

        public Fragment()
        {
            OutStates = new List<State>();
            StartState = null;
        }

        public List<State> OutStates { get; }

        public State StartState { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Start of fragment " + StartState);
            foreach (var s in OutStates) sb.AppendLine("State out " + s);
            return sb.ToString();
        }
    }
}