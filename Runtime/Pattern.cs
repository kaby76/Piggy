namespace Runtime
{
    using Antlr4.Runtime.Tree;
    using System.Collections.Generic;

    public class Pattern
    {
        private static int id = 0;
        private static Dictionary<int, Pattern> _handles = new Dictionary<int, Pattern>();
        public Pattern() { Id = ++id; _handles[Id] = this; }
        public Pass Owner { get; set; }
        public int Id { get; protected set; }
        public static Pattern GetPattern(int i)
        {
            _handles.TryGetValue(i, out Pattern result);
            return result;
        }
        public IParseTree AstNode { get; set; }
    }
}
