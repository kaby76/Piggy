
template Enums
{
    header {{
        protected bool first = true;
        protected string generated_file_name;
        protected int count = 1;
    }}

    pass GenerateEnums {
        ( EnumDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}"
            {{
                string name = tree.Attr("Name");
                first = true;
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

        ( EnumDecl SrcRange=$"{ClangSupport.limit}" !Name
            {{
                // Anonymous enum type. Several ways to deal with this.
                string name = null;
                // Check if an EnumConstantDecl child named is in the rewrite table
                // for anonymous types. If so, use that name and proceed.
                if (name == null)
                {
                    int pos = 0;
                    for (;;)
                    {
                        var c1 = tree.Child(pos++);
                        if (c1 == null) break;
                        var t1 = c1.Type();
                        if (t1 != "EnumConstantDecl") continue;
                        var s = c1.Attr("Name");
                        s = s.Trim();
                        ClangSupport._anonymous_enum_map.TryGetValue(s, out string v);
                        if (v == null) continue;
                        name = v;
                        break;
                    }
                }
                // Check if the declaration contains a comment description.
                if (name == null)
                {
                    for (;;)
                    {
                        var c1 = tree.Child(0);
                        if (c1 == null) break;
                        var t1 = c1.Type();
                        if (t1 != "FullComment") break;
                        var c2 = c1.Child(0);
                        if (c2 == null) break;
                        var t2 = c2.Type();
                        if (t2 != "ParagraphComment") break;
                        var c3 = c2.Child(0);
                        if (c3 == null) break;
                        var t3 = c3.Type();
                        if (t3 != "TextComment") break;
                        var comment = c3.Attr("Text");
                        comment = comment.Trim();
                        comment = comment
                            .Replace(",", " ")
                            .Replace("("," ")
                            .Replace(")"," ")
                            .Replace("-"," ")
                            .Replace("\\"," ")
                            .Replace("#"," ")
                            .Replace(";"," ")
                            .Replace("/"," ")
                            .Replace("_"," ")
                            .Replace(":"," ")
                            .Replace("<"," ")
                            .Replace(">"," ")
                            .Replace("{"," ")
                            .Replace("}"," ")
                            .Replace("@"," ")
                            .Replace("."," ")
                            ;
                        comment = Regex.Replace(comment, @"(^\w)|(\s\w)", w => w.Value.ToUpper());
                        comment = comment.Replace(" ", "");
                        comment = Regex.Replace(comment, @"(^\d+)", w => "_" + w.Value);
                        name = comment;
                        break;
                    }
                }
                if (name == null)
                {
                    var slist = tree.Children("EnumConstantDecl").Select(t => t.Attr("Name")).ToList();
                    var s = ClangSupport.CommonStringPrefix.Of(slist);
                    if (s != null && s != "")
                        name = s;
                }
                if (name == null)
                {
                    name = "generated";
                }
                first = true;
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
                    ClangSupport.FormatFile(generated_file_name);
                }
            }}
        )
    }
}