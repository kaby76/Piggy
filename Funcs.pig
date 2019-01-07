
template Funcs
{
    header {{
        protected bool first = true;
        protected string dllname = "";
    }}

	pass Start {
		( TranslationUnitDecl [[
		public class Functions {
		]])
	}

	pass End {
		( TranslationUnitDecl [[
		}
		]])
	}

    pass Functions {
        ( FunctionDecl SrcRange=$"{Funcs.limit}" Name=*
            {{
                result.Append("[DllImport(\"" + dllname + "\", CallingConvention = CallingConvention.ThisCall,"
                   + " EntryPoint=\"" + tree.Attr("Name") + "\")]" + Environment.NewLine);
                var scope = _stack.Peek();
				var function_type = tree.Attr("Type");
				var raw_return_type = PiggyRuntime.TemplateHelpers.GetFunctionReturn(function_type);
                var premod_type = raw_return_type;
                var postmod_type = PiggyRuntime.TemplateHelpers.ModNonParamUsageType(premod_type);
				var symbol = scope.getSymbol(postmod_type);
				string type = "";
                if (symbol == null)
                {
					type = "UNKNOWN";
                }
                else
                {
                    type = symbol.Name;
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
					var postmod_type = PiggyRuntime.TemplateHelpers.ModParamUsageType(premod_type);
                    result.Append(postmod_type + " " + tree.Attr("Name"));
                }}
            )*
            [[);

            ]]
        )
    }
}