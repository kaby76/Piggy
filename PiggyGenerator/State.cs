namespace PiggyGenerator
{
    using System.Linq;
    using System.Collections.Generic;

    public class State
    {
        public List<Edge> _out_edges;
        private Automaton _owner;
        private static int _next_id;
        public int Id { get; private set; }

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
        public bool IsFinalState()
        {
            return Owner.FinalStates.Contains(this);
        }
        public bool IsFinalStateSubpattern()
        {
            return Owner.FinalStatesSubpattern.Contains(this);
        }
        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
