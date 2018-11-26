
namespace Piggy
{
    using System;
    using System.Collections.Generic;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;

    public class Tree
    {
        IParseTree ast;
        IParseTree current;
        private TreeRegEx re;

        public Tree(TreeRegEx r, IParseTree t, IParseTree cur)
        {
            re = r;
            ast = t;
            current = cur;
            StackQueue<IParseTree> stack = new StackQueue<IParseTree>();
            var visited = new HashSet<IParseTree>();
            stack.Push(ast);
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (visited.Contains(v)) continue;
                if (v == cur) return;
                visited.Add(v);
                for (int i = v.ChildCount - 1; i >= 0; --i)
                {
                    var c = v.GetChild(i);
                    if (!visited.Contains(c))
                        stack.Push(c);
                }
            }
        }

        public Tree Peek(int level)
        {
            IParseTree v = current;
            for (int j = 0; j < level; ++j)
            {
                re.parent.TryGetValue(v, out IParseTree par);
                v = par;
            }
            Tree t = new Tree(re, ast, v);
            return t;
        }

        public object Attr(string name)
        {
            // Find attribute at this level and return value.
            int n = current.ChildCount;
            for (int i = 0; i < n; ++i)
            {
                var t = current.GetChild(i);
                var is_attr = re.is_ast_attr(t.GetChild(0));
                if (!is_attr) continue;
                int pos = 0;
                var t_id = t.GetChild(0).GetChild(pos);
                if (name != t_id.GetText()) continue;
                pos++;
                pos++;
                var t_val = t.GetChild(0).GetChild(pos);
                var str = t_val.GetText();
                var nstr = str.Substring(1).Substring(0, str.Length - 2);
                return nstr;
            }
            return "";
        }

        public string ChildrenOutput()
        {
            return "";
        }
    }
}
