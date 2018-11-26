
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
        StackQueue<IParseTree> stack = new StackQueue<IParseTree>();

        public Tree(IParseTree t, IParseTree cur)
        {
            ast = t;
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
            Tree t = new Tree(ast, stack.PeekTop(level));
            return t;
        }

        public object Attr(string name)
        {
            // Find attribute at this level and return value.
            int n = current.ChildCount;
            for (int i = 0; i < n; ++i)
            {
                var t = current.GetChild(i);
                AstParserParser.AttrContext t_attr =
                    t as AstParserParser.AttrContext;
                if (t_attr == null) continue;

                int pos = 0;
                var t_id = t_attr.GetChild(pos);
                if (name != t_id.GetText()) continue;

                pos++;
                pos++;

                var t_val = t_attr.GetChild(pos);
                return t_val.GetText();
            }
            return "";
        }

        public string ChildrenOutput()
        {
            return "";
        }
    }
}
