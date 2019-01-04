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
        public static SymbolTable _symbol_table = new SymbolTable();
    }}

    init {{
        _symbol_table.initTypeSystem();
        Scope scope = new GlobalScope(null);
        _stack.Push(scope);
    }}

    pass CollectEnums {
        ( TypedefDecl SrcRange=$"{Decls.limit}" Name=* ( ElaboratedType ( EnumType ( Enum Name=*
            {{
                var scope = _stack.Peek();
                var name = tree.Attr("Name");
				var typedef_name = tree.Peek(3).Attr("Name");
                if (typedef_name == "") return;
                if (scope.getSymbol(typedef_name) != null) return;
				var type = new TypeAlias(typedef_name, name);
                scope.define(type);
            }}
        ))))

        ( EnumDecl SrcRange=$"{Decls.limit}" Name=*
            {{
                var scope = _stack.Peek();
                var name = tree.Attr("Name");

                if (scope.getSymbol(name) != null) return;
                var type = new EnumSymbol(name);
                scope.define(type);
            }}
        )
    }
}