namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;
    using System.Collections.Generic;
    using System.Text;

    public class Edge
    {
        public enum EdgeModifiers
        {
            Exact = 0,
            Not = 1,
            Any = 2,
        }

        public State _from;
        public State _to;
        public IParseTree _c;
        public IParseTree _other;
        public bool _not;
        public bool _any;
        public string _c_text;
        public System.Type _c_type;
        private Automaton _owner;
        public int _edge_modifiers;

        public Edge(Automaton o, State f, State t, IParseTree c, IParseTree other, int edge_modifiers = 0)
        {
            _owner = o;
            _from = f;
            _to = t;
            _other = other;
            _c = c; // Null indicates empty string.
            if (_c != null)
            {
                _c_text = _c.GetText();
                _c_type = _c.GetType();
            }
            _edge_modifiers = edge_modifiers;
            _not = 0 != (edge_modifiers & (int)EdgeModifiers.Not);
            _any = 0 != (edge_modifiers & (int)EdgeModifiers.Any);
        }
        public void Commit()
        {
            _owner.AddEdge(this);
            _from._out_edges.Add(this);
        }
        public override int GetHashCode()
        {
            return _from.Id + _to.Id * 16;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var o = obj as Edge;
            if (o == null) return false;
            if (this._from != o._from || this._to != o._to) return false;
            if (this._c != o._c) return false;
            if (this._other != o._other) return false;
            return true;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this._from + " -> " + this._to + " on " + (this._c_text == null ? "empty" : this._c_text));
            return sb.ToString();
        }
    }
}
