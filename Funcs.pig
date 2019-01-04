
template Funcs
{
    header {{
        protected bool first = true;
        protected string limit = ""; // Context of what file can match.
	protected string dllname = "";
        protected Stack<Scope> _stack = new Stack<Scope>();
        public static SymbolTable _symbol_table = new SymbolTable();
    }}

    init {{
        _stack.Push(new GlobalScope(null));
    }}

    pass Functions {
        ( FunctionDecl SrcRange=$"{Funcs.limit}" Name=*
            {{
                result.Append("[DllImport(" + dllname + ", CallingConvention = CallingConvention.ThisCall,"
                   + " EntryPoint=\"" + tree.Attr("Name") + "\")]" + Environment.NewLine);
                result.Append("public static extern "
                   + PiggyRuntime.TemplateHelpers.GetFunctionReturn(tree.Attr("Type")) + " "
                   + tree.Attr("Name") + "(");
				result.AppendLine("");
                first = true;
            }}
            ( ParmVarDecl Name=* Type=*
                {{
                    if (first)
                        first = false;
                    else
                        result.Append(", ");
                    var premod_type = tree.Attr("Type");
                    var postmod_type = PiggyRuntime.TemplateHelpers.ModParamType(premod_type);
                    result.Append(postmod_type + " " + tree.Attr("Name"));
                }}
            )*
            [[);
            ]]
        )
    }
}