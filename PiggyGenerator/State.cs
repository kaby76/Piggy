﻿namespace PiggyGenerator
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
        private Automaton _owner;
        private static int _next_id;
        public int Id { get; private set; }

        /**
         * Initializes a new match state with no outgoing transitions.
         */
        public State(Automaton owner)
        {
            _owner = owner;
            Id = _next_id++;
            _out_edges = new List<Edge>();
        }
        public void Commit()
        {
            _owner.AddState(this);
        }
        public Automaton Owner
        {
            get
            {
                return _owner;
            }
        }

        public override int GetHashCode()
        {
            return Id;
        }

        /**
         * @return true, if the state is a match state.
         */
        public bool isMatch()
        {
            return Owner.EndStates.Contains(this);
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}