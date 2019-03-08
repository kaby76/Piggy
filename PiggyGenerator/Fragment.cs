using System;
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
        private State _in_state;
        private List<State> _out_states;

        /**
         * Initializes a new instance of Fragment, with starting state.
         * @param start The starting state of the fragment
         */
        public Fragment(State start)
        {
            _out_states = new List<State>();
            _out_states.Add(start); // start state is also and exit state.
            _in_state = start;
        }

        /**
         * Initializes a new instance of Fragment, with starting state and one outwards-pointing state.
         * @param start The starting state of the fragment
         * @param out The outwards-pointing state
         */
        public Fragment(State start, State @out)
        {
            _out_states = new List<State>();
            _out_states.Add(@out);
            _in_state = start;
        }

        /**
         * Initializes a new instance of Fragment, with starting state and a number of outwards-pointing
         * states, contained in an instance of OwnArrayList.
         * @param start The starting state of the fragment
         * @param outArrows The instance of OwnArrayList that contains the outwards-pointing states.
         */
        public Fragment(State start, List<State> out_states)
        {
            _out_states = out_states;
            _in_state = start;
        }

        /**
         * Initializes a new instance of Fragment with no starting state and no outwards pointing arrows,
         * creating in essence an empty fragment.
         */
        public Fragment()
        {
            _out_states = new List<State>();
            _in_state = null;
        }

        /**
         *
         * @return the starting state of the Fragment
         */
        public State StartState
        {
            get { return _in_state; }
        }

        /**
         *
         * @return the outwards pointing arrows contained in an instance of OwnArrayList
         */
        public List<State> OutStates
        {
            get { return _out_states; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Start of fragment " + this._in_state);
            foreach (var s in this._out_states)
            {
                sb.AppendLine("State out " + s);
            }
            return base.ToString();
        }
    }
}
