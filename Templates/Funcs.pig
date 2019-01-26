
template Funcs
{
    header {{
        protected bool first = true;
        protected struct generate_type {
            public string name;
            public System.Runtime.InteropServices.CallingConvention convention;
            public Dictionary<int, string> special_args;
        }
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
        ( TranslationUnitDecl
            {{
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this declaration.
                    var output_file_name = "g-functions.cs";
                    PiggyRuntime.Tool.GeneratedFiles.Add(output_file_name);
                    PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(output_file_name);
                    System.Console.WriteLine("namespace " + ClangSupport.namespace_name);
                    System.Console.WriteLine("{");
                    System.Console.WriteLine("using System;");
                    System.Console.WriteLine("using System.Runtime.InteropServices;");
                }
            }}
            [[
        public class Functions {
        ]]{{ System.Console.Write("const string DllName = \"" + ClangSupport.dllname + "\";" + Environment.NewLine); }}
        )
    }

    pass End {
        ( TranslationUnitDecl
            {{
				System.Console.WriteLine("}");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    System.Console.WriteLine("}");
                    PiggyRuntime.Tool.Redirect.Dispose();
                    PiggyRuntime.Tool.Redirect = null;
                    var output_file_name = "g-functions.cs";
                    ClangSupport.FormatFile(output_file_name);
                }
            }}
        )
    }

    pass Functions {
        ( FunctionDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}"
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
                System.Console.Write("[DllImport(DllName,"
                    + " CallingConvention = CallingConvention." + gt.convention.ToString() + ", "
                    + " EntryPoint=\"" + function_name + "\")]" + Environment.NewLine);
                var scope = _stack.Peek();
                var function_type = tree.Attr("Type");
                var raw_return_type = ClangSupport.GetFunctionReturn(function_type);
                var premod_type = raw_return_type;
                var postmod_type = ClangSupport.ModNonParamUsageType(premod_type);
                var type = postmod_type;
                System.Console.Write("public static extern "
                   + type + " "
                   + patch_up_function_name + "(");
                first = true;
            }}
            ( ParmVarDecl Name=* Type=*
                {{
                    if (first)
                        first = false;
                    else
                        System.Console.Write(", ");
                    var premod_type = tree.Attr("Type");
                    var postmod_type = ClangSupport.ModParamUsageType(premod_type);
                    var param_name = tree.Attr("Name");
                    var patch_up_param_name = ClangSupport.EscapeCsharpNames(param_name);
                    System.Console.Write(postmod_type + " " + patch_up_param_name);
                }}
            )*
            [[);

            ]]
        )
    }
}