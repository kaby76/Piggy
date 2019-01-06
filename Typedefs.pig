
template Typedefs
{
    header {{
        protected string limit = ""; // Context of what file can match.
        protected Stack<Scope> _stack = new Stack<Scope>();
    }}

    init {{
        _stack.Push(_symbol_table.GLOBALS);
    }}

    pass GenerateTypedefs {
        ( TypedefDecl SrcRange=$"{Typedefs.limit}" Name=* ( BuiltinType BareType=*
            {{
                var scope = _stack.Peek();
                var typedef_name = tree.Peek(1).Attr("Name");
                var baretype_name = tree.Attr("BareType");
				baretype_name = PiggyRuntime.TemplateHelpers.ModParamType(baretype_name);
                if (scope.getSymbol(typedef_name) != null) return;
                var sym = scope.getSymbol(baretype_name) as org.antlr.symtab.Type;
                if (sym == null)
				{
					sym = new PrimitiveType(baretype_name);
				}
                var type = new TypeAlias(typedef_name, sym);
                scope.define(type);
                result.AppendLine(
                    @"public partial struct " + typedef_name + @"
                    {
                        public " + typedef_name + @"(" + baretype_name + @" value)
                        {
                            this.Value = value;
                        }
                        public " + baretype_name + @" Value;
                    }
                    ");
            }}
        ))
    }
}