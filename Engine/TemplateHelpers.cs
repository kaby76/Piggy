using System.Collections.Generic;
using Antlr4.Runtime.Tree;
using Runtime;

namespace Engine
{
    public static class CodeHelper
    {
        public static Dictionary<IParseTree, string> CollectCode(this Pattern pattern)
        {
            var result = new Dictionary<IParseTree, string>();
            var visited = new HashSet<IParseTree>();
            var stack = new Stack<IParseTree>();
            stack.Push(pattern.AstNode);
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (visited.Contains(v))
                    continue;
                visited.Add(v);
                if (v as SpecParserParser.CodeContext != null)
                {
                    var code = v.GetText();
                    result.Add(v, code);
                    continue;
                }

                for (var i = v.ChildCount - 1; i >= 0; --i)
                {
                    var c = v.GetChild(i);
                    if (!visited.Contains(c))
                        stack.Push(c);
                }
            }

            return result;
        }

        public static Dictionary<IParseTree, string> CollectInterpolatedStringCode(this Pattern pattern)
        {
            var result = new Dictionary<IParseTree, string>();
            var visited = new HashSet<IParseTree>();
            var stack = new Stack<IParseTree>();
            stack.Push(pattern.AstNode);
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (visited.Contains(v))
                    continue;
                visited.Add(v);
                if (v as SpecParserParser.AttrContext != null)
                {
                    var p_attr = v as SpecParserParser.AttrContext;
                    var pos = 0;
                    var p_id = p_attr.GetChild(pos);
                    if (p_id.GetText() == "!")
                    {
                        pos++;
                        p_id = p_attr.GetChild(pos);
                    }

                    pos++;
                    pos++;
                    var p_val = p_attr.GetChild(pos);
                    if (p_val == null) continue;
                    var pp = p_val.GetText();
                    if (pp.StartsWith("$\"{"))
                    {
                        pp = pp.Substring(3);
                        pp = pp.Substring(0, pp.Length - 2);
                        var code = pp;
                        result.Add(p_val, code);
                    }

                    continue;
                }

                for (var i = v.ChildCount - 1; i >= 0; --i)
                {
                    var c = v.GetChild(i);
                    if (!visited.Contains(c))
                        stack.Push(c);
                }
            }

            return result;
        }
    }
}