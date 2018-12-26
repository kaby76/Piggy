namespace PiggyGenerator
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Tree;
    using org.antlr.symtab;
    using PiggyRuntime;

    public class AstSymtabBuilderListener : AstParserBaseListener
    {
        private IParseTree _ast;
        private Dictionary<IParseTree, IParseTree> _parent;
        private Stack<Scope> _stack;
        private SymbolTable _symbol_table;
        private Dictionary<IParseTree, org.antlr.symtab.Type> _types;
        private Scope _current_scope;

        public AstSymtabBuilderListener(IParseTree ast)
        {
            _ast = ast;
            _parent = Parents.Compute(ast);
            _stack = new Stack<Scope>();
            _symbol_table = new SymbolTable();
            _symbol_table.initTypeSystem();
            _types = new Dictionary<IParseTree, org.antlr.symtab.Type>();
            _current_scope = null;
        }

        public override void EnterDecl(AstParserParser.DeclContext context)
        {
            IParseTree id = context.GetChild(1);
            string name = id.GetText();
            switch (name)
            {
                case "TranslationUnitDecl":
                {
                    var t = new Tree(_parent, _ast, context);
                    Scope scope = new GlobalScope(null);
                    _stack.Push(scope);
                    break;
                }
                case "LinkageSpecDecl":
                {
                    var t = new Tree(_parent, _ast, context);
                    Scope scope = new LocalScope(_stack.Peek());
                    _stack.Push(scope);
                    break;
                }
                case "TypedefDecl":
                {
                    var t = new Tree(_parent, _ast, context);
                    var scope = _stack.Peek();
                    var c = t.Child(0);
                    this._types.TryGetValue(c, out org.antlr.symtab.Type cty);
                    string td_name = (string) t.Attr("Name");
                    if (scope.resolve(td_name) != null) break;
                    var typedef = new TypeAlias(td_name, cty);
                    scope.define(typedef);
                    break;
                }
                case "EnumDecl":
                {
                    var t = new Tree(_parent, _ast, context);
                    var scope = _stack.Peek();
                    var enum_name = (string)t.Attr("Name");
                    if (enum_name == "") break;
                    var typedef = new TypeAlias(enum_name, null);
                    scope.define(typedef);
                    break;
                }
            }
        }

        public override void ExitDecl(AstParserParser.DeclContext context)
        {
            IParseTree id = context.GetChild(1);
            string name = id.GetText();
            switch (name)
            {
                case "TranslationUnitDecl":
                {
                    _stack.Pop();
                    break;
                }
                case "LinkageSpecDecl":
                {
                    _stack.Pop();
                    break;
                }
                case "TypedefDecl":
                {
                    break;
                }
                case "EnumDecl":
                {
                    break;
                }
            }
        }
    }
}
