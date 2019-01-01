/* These templates collect basic information on typedefs, enums, structs, classes
 * for use later on in the program.
 */
template Decls
{
    header {{
        protected bool first = true;
        protected string limit = ".*"; // Context of what file can match.
        protected string dllname = "unknown_dll"; // A dll to load for DllImport.
        protected Stack<Scope> _stack = new Stack<Scope>();
        protected SymbolTable _symbol_table = new SymbolTable();
    }}

    init {{
        _symbol_table.initTypeSystem();
        Scope scope = new GlobalScope(null);
        _stack.Push(scope);
    }}

    pass CollectEnums {
        ( EnumDecl SrcRange=$"{Decls.limit}"
            {{
                // Grab the declaration. If no name, then create one.
                var scope = _stack.Peek();
                var name = tree.Attr("Name");
                if (name == "") return;
                if (scope.getSymbol(name) != null) return;
                var type = new EnumSymbol(name);
                scope.define(type);
            }}
        )

        ( TypedefDecl Name=* ( ElaboratedType ( EnumType ( Enum Name=*
            {{
                var scope = _stack.Peek();
                var name = tree.Attr("Name");
                if (name == "") return;
                if (scope.getSymbol(name) != null) return;
                var type = new TypeAlias(name, null);
                scope.define(type);
            }}
        ))))

    }
}