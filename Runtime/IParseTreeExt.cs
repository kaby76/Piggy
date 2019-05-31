using System.Collections.Generic;
using System.Collections;
using Antlr4.Runtime.Tree;

namespace Runtime
{
    public class EnumerableChildrenReverseIParseTree : IEnumerable<IParseTree>
    {
        private readonly IParseTree _start;

        public EnumerableChildrenReverseIParseTree(IParseTree start)
        {
            _start = start;
        }

        public IEnumerator<IParseTree> GetEnumerator()
        {
            return Doit();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Doit();
        }

        private IEnumerator<IParseTree> Doit()
        {
            for (var i = _start.ChildCount - 1; i >= 0; --i)
            {
                var c = _start.GetChild(i);
                yield return c;
            }
        }
    }

    public class EnumerableChildrenForwardIParseTree : IEnumerable<IParseTree>
    {
        private readonly IParseTree _start;

        public EnumerableChildrenForwardIParseTree(IParseTree start)
        {
            _start = start;
        }

        public IEnumerator<IParseTree> GetEnumerator()
        {
            return Doit();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Doit();
        }

        private IEnumerator<IParseTree> Doit()
        {
            for (var i = _start.ChildCount - 1; i >= 0; --i)
            {
                var c = _start.GetChild(i);
                yield return c;
            }
        }
    }

    public class PostorderIParseTree : IEnumerable<IParseTree>
    {
        private readonly IParseTree _start;

        public PostorderIParseTree(IParseTree start)
        {
            _start = start;
        }

        public IEnumerator<IParseTree> GetEnumerator()
        {
            return Doit();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Doit();
        }

        private IEnumerator<IParseTree> Doit()
        {
            var stack = new Stack<IParseTree>();
            var visited = new HashSet<IParseTree>();
            stack.Push(_start);
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (visited.Contains(v))
                {
                    yield return v;
                }
                else
                {
                    stack.Push(v);
                    visited.Add(v);
                    for (var i = v.ChildCount - 1; i >= 0; --i)
                    {
                        var c = v.GetChild(i);
                        stack.Push(c);
                    }
                }
            }
        }
    }

    public class PreorderIParseTree : IEnumerable<IParseTree>
    {
        private readonly IParseTree _start;

        public PreorderIParseTree(IParseTree start)
        {
            _start = start;
        }

        public IEnumerator<IParseTree> GetEnumerator()
        {
            return Doit();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Doit();
        }

        private IEnumerator<IParseTree> Doit()
        {
            var stack = new Stack<IParseTree>();
            stack.Push(_start);
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                yield return v;
                if (v as AstParserParser.NodeContext != null)
                {
                    var n = v.GetChild(1);
                }
                for (int i = v.ChildCount - 1; i >= 0; --i)
                {
                    var c = v.GetChild(i);
                    stack.Push(c);
                }
            }
        }
    }

    public static class IParseTreeExt
    {
        public static IEnumerable<IParseTree> ChildrenReverse(this IParseTree tree)
        {
            return new EnumerableChildrenReverseIParseTree(tree);
        }

        public static IEnumerable<IParseTree> ChildrenForward(this IParseTree tree)
        {
            return new EnumerableChildrenForwardIParseTree(tree);
        }

        public static IEnumerable<IParseTree> Preorderx(this IParseTree tree)
        {
            return new PreorderIParseTree(tree);
        }

        public static IEnumerable<IParseTree> Postorder(this IParseTree tree)
        {
            return new PostorderIParseTree(tree);
        }

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
