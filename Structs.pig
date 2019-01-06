template Structs
{
    header {{
        protected bool first = true;
        protected string limit = ""; // Context of what file can match.
        protected Stack<Scope> _stack = new Stack<Scope>();
		protected List<string> do_not_match_these = new List<string>();
    }}

    init {{
        _stack.Push(_symbol_table.GLOBALS);
    }}

    pass CollectStructs {
        ( TypedefDecl SrcRange=$"{Structs.limit}" Name=* (* CXXRecord Name=*
            {{
                var scope = _stack.Peek();
                // Get the name of the typedef.
                var name = tree.Attr("Name");
                // Challenge here with Kleene Closure--I don't know apriori at
                // what level the TypedefDecl was matched, so I don't know what level
                // to do the peek. Piggy needs work. For now, just search
                // up the tree for the right type.
                int decl_level = -1;
                var decl = tree;
                for (int i = 0; ; ++i)
                {
                    var p = decl.Current;
                    if (p == null)
                        break;
                    if (p.GetChild(1).GetText() == "TypedefDecl")
                    {
                        decl_level = i;
                        break;
                    }
                    if (p == null)
                        break;
                    decl = decl.Peek(1);
                    if (decl == null)
                        break;
                }
                if (decl_level == -1) return;
                var typedef_name = tree.Peek(decl_level).Attr("Name");
                if (typedef_name == "") return;
				// Kleene star can potentially match too much. Remove these
				// from consideration another way.
				if (do_not_match_these.Contains(typedef_name)) return;
				if (do_not_match_these.Contains(name)) return;
                var def = scope.getSymbol(typedef_name);
                if (def != null) return;
                var sym = scope.getSymbol(name);
                if (sym == null)
                {
                    sym = new StructSymbol(name);
                    scope.define(sym);
                }
                var type = new TypeAlias(typedef_name, sym as org.antlr.symtab.Type);
                scope.define(type);
            }}
        *) )
    }

    pass GenerateStructs {
		// If there are fields, set them up.
        ( CXXRecordDecl SrcRange=$"{Structs.limit}" Name=*
            {{
                string name = tree.Attr("Name");
                var scope = _stack.Peek();
                var typedef_name = scope.resolve(name, true);
                if (typedef_name != null) name = typedef_name.Name;
                result.AppendLine(
                    @"public partial struct " + name + @"
                    {
                    ");
            }}
			(%
                ( FieldDecl
                    {{
						var type = tree.Attr("Type");
						var name = tree.Attr("Name");
                        result.AppendLine("" + type + " " + name + ";");
                    }}
                )
            %)*
            [[}
            ]]
        )

		// If no fields, make a plain struct.
        ( CXXRecordDecl SrcRange=$"{Structs.limit}" Name=*
            {{
                string name = tree.Attr("Name");
                var scope = _stack.Peek();
                var typedef_name = scope.resolve(name, true);
                if (typedef_name != null) name = typedef_name.Name;
                result.AppendLine(
                    @"public partial struct " + name + @"
                    {
                        public " + name + @"(IntPtr pointer)
                        {
                            this.Pointer = pointer;
                        }
                        public IntPtr Pointer;
                    }
                    ");
            }}
        )
    }
}