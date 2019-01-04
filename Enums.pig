
template Enums
{
    header {{
        protected bool first = true;
        protected string limit = ""; // Context of what file can match.
        protected Stack<Scope> _stack = new Stack<Scope>();
        public static SymbolTable _symbol_table = new SymbolTable();
    }}

    init {{
        _stack.Push(new GlobalScope(null));
    }}

    pass CollectTypedefEnums {
        // Create enum types for use with typedefs.
        ( EnumDecl SrcRange=$"{Enums.limit}" Name=*
            {{
                var scope = _stack.Peek();
                var name = tree.Attr("Name");
                if (scope.getSymbol(name) != null) return;
                var type = new EnumSymbol(name);
                scope.define(type);
            }}
        )

        // These occur after the EnumDecl.
        ( TypedefDecl SrcRange=$"{Enums.limit}" Name=* ( ElaboratedType ( EnumType ( Enum Name=*
            {{
                var scope = _stack.Peek();
                var name = tree.Attr("Name");
                var typedef_name = tree.Peek(3).Attr("Name");
                if (typedef_name == "") return;
                var def = scope.getSymbol(typedef_name);
                if (def != null) return;
                var sym = scope.getSymbol(name) as org.antlr.symtab.Type;
                if (sym == null) return;
                var type = new TypeAlias(typedef_name, sym);
                scope.define(type);
            }}
        ))))
    }

    pass GenerateEnums {
        ( EnumDecl SrcRange=$"{Enums.limit}" Name=*
            {{
                first = true;
                string name = tree.Attr("Name");
                var scope = _stack.Peek();
                var typedef_name = scope.resolve(name, true);
                if (typedef_name != null) name = typedef_name.Name;
                result.Append("public enum @" + name + " {" + Environment.NewLine);
            }}
            (%
                ( EnumConstantDecl
                    ( IntegerLiteral
                        {{
                            if (first)
                                first = false;
                            else
                                result.Append("," + Environment.NewLine);
                            result.Append("@" + tree.Peek(1).Attr("Name") + " = " + tree.Peek(0).Attr("Value"));
                        }}
                    )
                )
                |
                ( EnumConstantDecl
                    {{
                        if (first)
                            first = false;
                        else
                            result.Append("," + Environment.NewLine);
                        result.Append("@" + tree.Attr("Name"));
                    }}
                )
            %)*
            [[}
            ]]
        )
    }
}