
template Typedefs
{
    // The only mechanism to create an alias type in C# is to use a struct,
    // where there is one field of the type desired.
	//
	// There are so far the following cases:

    pass GeneratePointerTypes {
        ( TypedefDecl SrcRange=$"{Typedefs.limit}" Name=* ( PointerType )
            {{
                var scope = _stack.Peek();
                var name = tree.Attr("Name");
                var baretype_name = "IntPtr";
                var def = scope.getSymbol(name);
                if (def != null) return;
                def = new StructSymbol(name);
                scope.define(def);
                result.AppendLine(
                    @"public partial struct " + name + @"
                    {
                        public " + name + @"(" + baretype_name + @" value)
                        {
                            this.Value = value;
                        }
                        public " + baretype_name + @" Value;
                    }
                    ");
            }}
        )
    }

    pass GenerateTypedefs {
        ( TypedefDecl SrcRange=$"{Typedefs.limit}" Name=* ( BuiltinType BareType=*
            {{
                var scope = _stack.Peek();
                var name = tree.Peek(1).Attr("Name");
                var baretype_name = tree.Attr("BareType");
                // Bare type could be pointer, etc., so apply modifications to get C# corrected type.
                baretype_name = PiggyRuntime.TemplateHelpers.ModNonParamUsageType(baretype_name);
                if (scope.getSymbol(name) != null) return;
                var sym = scope.getSymbol(baretype_name);
                if (sym == null)
                {
                    sym = new PrimitiveType(baretype_name) as org.antlr.symtab.Symbol;
					scope.define(sym);
                }
                var def = new StructSymbol(name);
                scope.define(def);
                result.AppendLine(
                    @"public partial struct " + name + @"
                    {
                        public " + name + @"(" + baretype_name + @" value)
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