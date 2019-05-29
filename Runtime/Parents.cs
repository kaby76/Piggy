namespace Runtime
{
    using Antlr4.Runtime.Tree;
    using System.Collections.Generic;

    public static class TreeInfo
    {
        private static Dictionary<IParseTree, IParseTree> _cached_parents;
        private static Dictionary<string, IParseTree> _cached_aggregate_types;
        private static List<IParseTree> _preorder;

        private static void Compute(IParseTree ast)
        {
            _cached_parents = new Dictionary<IParseTree, IParseTree>();
            _cached_aggregate_types = new Dictionary<string, IParseTree>();
            _preorder = new List<IParseTree>();
            var stack = new Stack<IParseTree>();
            stack.Push(ast);
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                _preorder.Add(v);
                if (v as AstParserParser.NodeContext != null)
                {
                    var n = v.GetChild(1);
                    _cached_aggregate_types[n.GetText()] = v;
                }
                for (int i = v.ChildCount - 1; i >= 0; --i)
                {
                    var c = v.GetChild(i);
                    _cached_parents[c] = v;
                    stack.Push(c);
                }
            }
        }

        public static Dictionary<IParseTree, IParseTree> Parents(this IParseTree ast)
        {
            if (_cached_parents == null)
                Compute(ast);
            return _cached_parents;
        }

        public static Dictionary<string, IParseTree> AggregateTypes(this IParseTree ast)
        {
            if (_cached_parents == null)
                Compute(ast);
            return _cached_aggregate_types;
        }

        public static List<IParseTree> Preorder(this IParseTree ast)
        {
            if (_cached_parents == null)
                Compute(ast);
            return _preorder;
        }
    }
}
