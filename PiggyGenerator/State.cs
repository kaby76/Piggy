namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;
    using System.Linq;
    using System.Collections.Generic;

    /**
     * More or less, Cox's State (https://swtch.com/~rsc/regexp/nfa.c.txt).
     */
    public class State
    {
        public List<Edge> _out_edges;
        public bool _match;
        private NFA _owner;
        private static int _next_id;
        public int _id;

        /**
         * Initializes a new split state with two outgoing free transitions to the given two states.
         * @param out1 The state in the end of the first free transition
         * @param out2 The state in the end of the second free transition
         */
        /**
         * Initializes a new match state with no outgoing transitions.
         */
        public State(NFA owner)
        {
            _match = false;
            _owner = owner;
            _id = _next_id++;
            _out_edges = new List<Edge>();
            owner._all_states.Add(this);
        }

        /**
         * @return true, if the state is a match state.
         */
        public bool isMatch()
        {
            return _match;
        }

        public override string ToString()
        {
            return this._id.ToString();
        }
    }
}
