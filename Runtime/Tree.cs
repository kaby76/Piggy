namespace Runtime
{
    using System.Collections.Generic;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;

    public class Tree
    {
        private readonly IParseTree _ast;
        private readonly IParseTree _current;
        private readonly Dictionary<IParseTree, IParseTree> _parent_map;
        private readonly CommonTokenStream _common_token_stream;

        public Tree(Dictionary<IParseTree, IParseTree> parent_map, IParseTree ast, IParseTree current, CommonTokenStream common_token_stream)
        {
            _parent_map = parent_map;
            _ast = ast;
            _current = current;
            _common_token_stream = common_token_stream;
        }

        public Tree Peek(int level)
        {
            IParseTree v = _current;
            if (level > 0)
            {
                while (v != null)
                {
                    _parent_map.TryGetValue(v, out IParseTree par);
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
            Tree t = new Tree(_parent_map, _ast, v, _common_token_stream);
            return t;
        }

        public IParseTree Current
        {
            get { return _current; }
        }

        public CommonTokenStream CommonTokenStream
        {
            get { return _common_token_stream; }
        }

        public string Attr(string name)
        {
            // Find attribute at this level and return value.
            int n = _current.ChildCount;
            for (int i = 0; i < n; ++i)
            {
                var t = _current.GetChild(i);
                AstParserParser.AttrContext attr = t as AstParserParser.AttrContext;
                var is_attr = attr != null;
                if (!is_attr) continue;
                int pos = 0;
                var t_id = t.GetChild(pos);
                if (name != t_id.GetText()) continue;
                pos++;
                pos++;
                var t_val = t.GetChild(pos);
                var str = t_val.GetText();
                var nstr = str.Substring(1).Substring(0, str.Length - 2);
                return nstr;
            }
            return "";
        }

        public Tree Child(int index)
        {
            Tree result = null;
            // Walk forward until next tree node found.
            int n = _current.ChildCount;
            for (int i = 0; i < n; ++i)
            {
                var t = _current.GetChild(i);
                AstParserParser.NodeContext decl = t as AstParserParser.NodeContext;
                var is_decl = decl != null;
                if (!is_decl) continue;
                if (index > 0)
                {
                    index--;
                    continue;
                }
                Tree tt = new Tree(_parent_map, _ast, decl, _common_token_stream);
                return tt;
            }
            return result;
        }

        public string Type()
        {
            string result = "";
            int n = _current.ChildCount;
            if (n < 1) return result;
            var t = _current.GetChild(1);
            return t.GetText();
        }

        public List<Tree> Children(string name)
        {
            List<Tree> result = new List<Tree>();
            // Walk forward until next tree node found.
            int n = _current.ChildCount;
            for (int i = 0; i < n; ++i)
            {
                var t = _current.GetChild(i);
                AstParserParser.NodeContext decl = t.GetChild(0) as AstParserParser.NodeContext;
                var is_decl = decl != null;
                if (!is_decl) continue;
                var u = decl.GetChild(1);
                string v = u.GetText();
                if (v != name) continue;
                Tree tt = new Tree(_parent_map, _ast, decl, _common_token_stream);
                result.Add(tt);
            }
            return result;
        }

        public string ChildrenOutput()
        {
            return "";
        }
    }
}
