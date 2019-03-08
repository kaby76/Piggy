namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;

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
        public bool _not;
        public bool _any;
        public string _c_text;
        public System.Type _c_type;
        private NFA _owner;

        public Edge(NFA o, State f, State t, IParseTree c, int edge_modifiers = 0)
        {
            _owner = o;
            _owner._all_edges.Add(this);
            f._out_edges.Add(this);
            _from = f;
            _to = t;
            _c = c; // Null indicates empty string.
            if (_c != null)
            {
                _c_text = _c.GetText();
                _c_type = _c.GetType();
            }
            _not = 0 != (edge_modifiers & (int)EdgeModifiers.Not);
            _any = 0 != (edge_modifiers & (int)EdgeModifiers.Any);
        }
    }
}
