
template Enums
{
    header {{
        protected bool first = true;
    }}

    pass GenerateEnums {
        ( EnumDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}"
            {{
                first = true;
                string name = tree.Attr("Name");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this declaration.
                    var output_file_name = "g-" + name + ".cs";
                    PiggyRuntime.Tool.GeneratedFiles.Add(output_file_name);
                    PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(output_file_name);
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
            [[}
            ]]
            {{
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    System.Console.WriteLine("}");
                    PiggyRuntime.Tool.Redirect.Dispose();
                    PiggyRuntime.Tool.Redirect = null;
                    string name = tree.Attr("Name");
                    var output_file_name = "g-" + name + ".cs";
                    ClangSupport.FormatFile(output_file_name);
                }
            }}
        )
    }
}