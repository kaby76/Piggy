namespace PiggyGenerator
{
    using System.Linq;

    public class State
    {
        private readonly Automaton _owner;
        private static int _next_id;
        private readonly int _id;
        public int Id
        {
            get { return _id; }
        }

        public State(Automaton owner)
        {
            _id = _next_id++;
            _owner = owner;
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
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(State)) return false;
            var o = obj as State;
            if (this.Id != o.Id) return false;
            return true;
        }
        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
