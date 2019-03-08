namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;

    public class Edge
    {
        public State _from;
        public State _to;
        private IParseTree _c;
        private bool _not;
        public string _c_text;
        private System.Type _c_type;
        private NFA _owner;

        public Edge(NFA o, State f, State t, IParseTree c, bool not = false)
        {
            _owner = o;
            _owner._all_edges.Add(this);
            _from = f;
            _to = t;
            _c = c; // Null indicates empty string.
            if (_c != null)
            {
                _c_text = _c.GetText();
                _c_type = _c.GetType();
            }
            _not = not;
        }
    }
}
