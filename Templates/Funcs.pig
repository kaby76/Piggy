
template Funcs
{
    header {{
        protected bool first = true;
        protected string dllname = "";
        protected struct generate_type {
            public string name;
            public System.Runtime.InteropServices.CallingConvention convention;
            public Dictionary<int, string> special_args;
        }
        protected string generate_for_only = ".*";
        protected List<generate_type> details
            = new List<generate_type>()
            {
                { new generate_type()
                    {
                        name = ".*",
                        convention = System.Runtime.InteropServices.CallingConvention.Cdecl,
                        special_args = null
                    }
                }
            }; // default for everything.
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
        ( FunctionDecl SrcRange=$"{Funcs.limit}" Name=$"{Funcs.generate_for_only}"
            {{
				var function_name = tree.Attr("Name");
				var gt = details.Where(d => 
					{
						Regex regex = new Regex("(?<exp>" + d.name + ")");
                        var match = regex.Match(function_name);
						if (match.Success)
							return true;
						else
							return false;
					}).First();
                result.Append("[DllImport(\"" + dllname + "\","
					+ " CallingConvention = CallingConvention." + gt.convention.ToString() + ", "
                    + " EntryPoint=\"" + function_name + "\")]" + Environment.NewLine);
                var scope = _stack.Peek();
                var function_type = tree.Attr("Type");
                var raw_return_type = PiggyRuntime.TemplateHelpers.GetFunctionReturn(function_type);
                var premod_type = raw_return_type;
                var postmod_type = PiggyRuntime.TemplateHelpers.ModNonParamUsageType(premod_type);
                var type = postmod_type;
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