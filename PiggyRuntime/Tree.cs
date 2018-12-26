
namespace PiggyRuntime
{
    using System;
    using System.Collections.Generic;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;

    public class Tree
    {
        IParseTree _ast;
        IParseTree _current;
        Dictionary<IParseTree, IParseTree> _parent;

        public Tree(Dictionary<IParseTree, IParseTree> parent, IParseTree ast, IParseTree current)
        {
            _parent = parent;
            _ast = ast;
            _current = current;
        }

        public Tree Peek(int level)
        {
            IParseTree v = _current;
            if (level > 0)
            {
                while (v != null)
                {
                    _parent.TryGetValue(v, out IParseTree par);
                    if (par == null)
                    {
                        v = null;
                        break;
                    }
                    if (v.GetText() != par.GetText())
                    {
                        v = par;
                        level--;
                        if (level == 0)
                            break;
                    }
                    v = par;
                }
            }
            Tree t = new Tree(_parent, _ast, v);
            return t;
        }

        public object Attr(string name)
        {
            // Find attribute at this level and return value.
            int n = _current.ChildCount;
            for (int i = 0; i < n; ++i)
            {
                var t = _current.GetChild(i);
                AstParserParser.AttrContext attr = t.GetChild(0) as AstParserParser.AttrContext;
                var is_attr = attr != null;
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

        public IParseTree Child(int index)
        {
            IParseTree result = null;
            // Walk forward until next tree node found.
            int n = _current.ChildCount;
            for (int i = 0; i < n; ++i)
            {
                var t = _current.GetChild(i);
                AstParserParser.DeclContext decl = t.GetChild(0) as AstParserParser.DeclContext;
                var is_decl = decl != null;
                if (!is_decl) continue;
                if (index > 0) continue;
                return decl;
            }
            return result;
        }


        public string ChildrenOutput()
        {
            return "";
        }
    }
}
