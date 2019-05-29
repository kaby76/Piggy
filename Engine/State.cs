namespace Engine
{
    public class State
    {
        private static int _next_id;

        public State(Automaton owner)
        {
            Id = _next_id++;
            Owner = owner;
            Owner.AddState(this);
        }

        public int Id { get; }

        public Automaton Owner { get; }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(State)) return false;
            var o = obj as State;
            if (Id != o.Id) return false;
            return true;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}