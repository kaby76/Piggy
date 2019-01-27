namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;
    using org.antlr.symtab;
    using PiggyRuntime;
    using System.Collections.Generic;

    public class AstSymtabBuilderListener : AstParserBaseListener
    {
        private IParseTree _ast;
        private Scope _current_scope;
        private Dictionary<IParseTree, IParseTree> _parent;
        private Stack<Scope> _stack;
        private SymbolTable _symbol_table;
        private Dictionary<IParseTree, org.antlr.symtab.Type> _types;

        public AstSymtabBuilderListener(IParseTree ast)
        {
            _ast = ast;
            _current_scope = null;
            _parent = Parents.Compute(ast);
            _stack = new Stack<Scope>();
            _symbol_table = new SymbolTable();
            _symbol_table.initTypeSystem();
            _types = new Dictionary<IParseTree, org.antlr.symtab.Type>();
        }
    }
}
