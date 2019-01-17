
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
                var patch_up_function_name = ClangSupport.EscapeCsharpNames(function_name);
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
                var raw_return_type = ClangSupport.GetFunctionReturn(function_type);
                var premod_type = raw_return_type;
                var postmod_type = ClangSupport.ModNonParamUsageType(premod_type);
                var type = postmod_type;
                result.Append("public static extern "
                   + type + " "
                   + patch_up_function_name + "(");
                first = true;
            }}
            ( ParmVarDecl Name=* Type=*
                {{
                    if (first)
                        first = false;
                    else
                        result.Append(", ");
                    var premod_type = tree.Attr("Type");
                    var postmod_type = ClangSupport.ModParamUsageType(premod_type);
                    var param_name = tree.Attr("Name");
                    var patch_up_param_name = ClangSupport.EscapeCsharpNames(param_name);
                    result.Append(postmod_type + " " + patch_up_param_name);
                }}
            )*
            [[);

            ]]
        )
    }
}