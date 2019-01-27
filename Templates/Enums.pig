
template Enums
{
    header {{
        protected bool first = true;
        protected string generated_file_name;
    }}

    pass GenerateEnums {
        ( EnumDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}"
            {{
                first = true;
                string name = tree.Attr("Name");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this declaration.
                    generated_file_name = PiggyRuntime.Tool.MakeFileNameUnique(PiggyRuntime.Tool.OutputLocation + "g-" + name + ".cs");
                    PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(generated_file_name);
                    System.Console.WriteLine("namespace " + ClangSupport.namespace_name);
                    System.Console.WriteLine("{");
                    System.Console.WriteLine("using System;");
                    System.Console.WriteLine("using System.Runtime.InteropServices;");
                }

                System.Console.Write("public enum " + name + " {" + Environment.NewLine);
            }}
            (%
                ( EnumConstantDecl
                    ( IntegerLiteral
                        {{
                            if (first)
                                first = false;
                            else
                                System.Console.Write("," + Environment.NewLine);
                            System.Console.Write("" + tree.Peek(1).Attr("Name") + " = " + tree.Peek(0).Attr("Value"));
                        }}
                    )
                )
                |
                ( EnumConstantDecl
                    {{
                        if (first)
                            first = false;
                        else
                            System.Console.Write("," + Environment.NewLine);
                        System.Console.Write("" + tree.Attr("Name"));
                    }}
                )
            %)*
            {{
                System.Console.WriteLine("}");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    System.Console.WriteLine("}");
                    PiggyRuntime.Tool.Redirect.Dispose();
                    PiggyRuntime.Tool.Redirect = null;
                    string name = tree.Attr("Name");
                    ClangSupport.FormatFile(generated_file_name);
                }
            }}
        )
    }
}