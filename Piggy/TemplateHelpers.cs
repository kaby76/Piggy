namespace Piggy
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Tree;
    using PiggyRuntime;

    public static class CodeHelper
    {
        public static Dictionary<IParseTree, string> CollectCode(this Pattern pattern)
        {
            Dictionary<IParseTree, string> result = new Dictionary<IParseTree, string>();
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
                for (int i = v.ChildCount - 1; i >= 0; --i)
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
