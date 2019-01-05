
template Funcs
{
    header {{
        protected bool first = true;
        protected string limit = ""; // Context of what file can match.
		protected string dllname = "";
        protected Stack<Scope> _stack = new Stack<Scope>();
		protected Dictionary<string, string> _parm_type_map = new Dictionary<string, string>();
    }}

    init {{
        _stack.Push(_symbol_table.GLOBALS);
    }}

    pass Functions {
        ( FunctionDecl SrcRange=$"{Funcs.limit}" Name=*
            {{
                result.Append("[DllImport(\"" + dllname + "\", CallingConvention = CallingConvention.ThisCall,"
                   + " EntryPoint=\"" + tree.Attr("Name") + "\")]" + Environment.NewLine);
                var scope = _stack.Peek();
				var type = tree.Attr("Type");
				var found_type = scope.getSymbol(type);
				if (found_type == null)
				{
					type = PiggyRuntime.TemplateHelpers.GetFunctionReturn(type);
				}
				else
				{
					type = found_type.Name;
				}
				result.Append("public static extern "
                   + type + " "
                   + tree.Attr("Name") + "(");
                first = true;
            }}
            ( ParmVarDecl Name=* Type=*
                {{
                    if (first)
                        first = false;
                    else
                        result.Append(", ");
                    var premod_type = tree.Attr("Type");
					_parm_type_map.TryGetValue(premod_type, out string postmod_type);
					if (postmod_type == null) postmod_type = PiggyRuntime.TemplateHelpers.ModParamType(premod_type);
                    result.Append(postmod_type + " " + tree.Attr("Name"));
                }}
            )*
            [[);
            ]]
        )
    }
}