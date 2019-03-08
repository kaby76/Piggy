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
        private List<Edge> _out_edges;
        private bool _match;
        private bool _hasChar;
        private int _lastlist;
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
            _hasChar = false;
            _match = true;
            _lastlist = -1;
            _owner = owner;
            _id = _next_id++;
            owner._all_states.Add(this);
        }

        /**
         * @return true, if the state is a literal state, otherwise false
         */
        public bool isLiteralState()
        {
            return _hasChar;
        }

        /**
         * @return true, if the state is a match state.
         */
        public bool isMatch()
        {
            return _match;
        }

        /**
         * @return The last generation of the current states list this state has been added to. If the state hasn't been added to any list yet, returns -1.
         */
        public int getLastlist()
        {
            return _lastlist;
        }

        /**
         * Sets the lastlist variable to the specified parameter
         * @param listindex the generation number of a list of current states
         */
        public void setLastlist(int listindex)
        {
            _lastlist = listindex;
        }

        public override string ToString()
        {
            return this._id.ToString();
        }
    }
}
