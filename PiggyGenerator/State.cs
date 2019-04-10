namespace PiggyGenerator
{
    using System.Linq;
    using System.Collections.Generic;

    /**
     * More or less, Cox's State (https://swtch.com/~rsc/regexp/nfa.c.txt).
     */
    public class State
    {
        public List<Edge> _out_edges;
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
            if (_owner.AllStates().Contains(this))
                return;
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
        public bool IsFinalState()
        {
            return Owner.EndStates.Contains(this);
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
