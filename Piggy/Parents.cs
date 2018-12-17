using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;

namespace Piggy
{
    public class Parents
    {
        private static Dictionary<IParseTree, Dictionary<IParseTree, IParseTree>> _cache = new Dictionary<IParseTree, Dictionary<IParseTree, IParseTree>>();

        public static Dictionary<IParseTree, IParseTree> Compute(IParseTree ast)
        {
            if (_cache.TryGetValue(ast, out Dictionary<IParseTree, IParseTree> result))
                return result;
            Dictionary<IParseTree, IParseTree> parent = new Dictionary<IParseTree, IParseTree>();
            var visited = new HashSet<IParseTree>();
            var stack = new Stack<IParseTree>();
            stack.Push(ast);
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (visited.Contains(v))
                    continue;
                visited.Add(v);
                for (int i = v.ChildCount - 1; i >= 0; --i)
                {
                    var c = v.GetChild(i);
                    parent[c] = v;
                    if (!visited.Contains(c))
                        stack.Push(c);
                }
            }
            _cache[ast] = parent;
            return parent;
        }
    }
}
