using System.Collections.Generic;

namespace PiggyRuntime
{
    using Antlr4.Runtime.Tree;

    public class Pattern
    {
        public Pattern() { }

        public Pass Owner { get; set; }

        public IParseTree AstNode { get; set; }

    }
}
