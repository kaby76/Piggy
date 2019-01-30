template Structs
{
    header {{
        protected bool first = true;
        int generated = 0;
        int offset;
        protected string generated_file_name;
    }}

    pass GenerateStructs {
        
        ( TranslationUnitDecl
        {{
            // Let's create "generate_for_only" to contain itself, plus Zero-width negative lookahead assertion
            // regular expression for each element in ClangSupport _name_map.
            StringBuilder sb = new StringBuilder();
            foreach (var t in ClangSupport._type_map)
            {
                var k = t.Key;
                sb.Append("(?!" + k + ")");
            }
            sb.Append(ClangSupport.generate_for_only);
            ClangSupport.generate_for_only = sb.ToString();         
        }}
        )
        
        ( CXXRecordDecl SrcRange=$"{ClangSupport.limit}" KindName=* Name=$"{ClangSupport.generate_for_only}" Attrs="definition"
            {{
                string name = tree.Attr("Name");
                string preferred_name = ClangSupport.RewriteAppliedOccurrence(false, name);
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this struct.
                    generated_file_name = PiggyRuntime.Tool.MakeFileNameUnique(PiggyRuntime.Tool.OutputLocation + "g-" + preferred_name + ".cs");
                    PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(generated_file_name);
                    System.Console.WriteLine("// This file generated by Piggy. Do not edit.");
                    System.Console.WriteLine("namespace " + ClangSupport.namespace_name);
                    System.Console.WriteLine("{");
                    System.Console.WriteLine("using System;");
                    System.Console.WriteLine("using System.Runtime.InteropServices;");
                }
                var scope = _stack.Peek();
                var layout = tree.Attr("KindName") == "struct"
                    ? @"[StructLayout(LayoutKind.Sequential)]"
                    : @"[StructLayout(LayoutKind.Explicit)]";
                offset = 0;
                System.Console.WriteLine(
                    layout + @"
                    public partial struct " + preferred_name + @"
                    {");
            }}
            ( FieldDecl
                {{
                    var name = tree.Attr("Name");
                    var premod_type = tree.Attr("Type");
                    Regex regex = new Regex("(?<basetype>.*)[[](?<index>.*)[]]");
                    var match = regex.Match(premod_type);
                    if (match.Success)
                    {
                        var ssize = (string)match.Groups["index"].Value;
                        if (ssize == "")
                        {
                            System.Console.WriteLine("public " + "IntPtr" + " gen" + generated++ + ";");
                        }
                        else 
                        {
                            var num = Int32.Parse(ssize);
                            var basetype = (string)match.Groups["basetype"].Value;
                            var base_postmod_type = ClangSupport.RewriteAppliedOccurrence(false, basetype);
                            for (int i = 0; i < num; ++i)
                            {
                                System.Console.WriteLine("public " + base_postmod_type + " gen" + generated++ + ";");
                            }
                        }
                    }
                    else
                    {
                        var postmod_type = ClangSupport.RewriteAppliedOccurrence(false, premod_type);
                        if (tree.Peek(1).Attr("KindName") == "union")
                            System.Console.WriteLine(@"[FieldOffset(" + offset + ")]");
                        System.Console.WriteLine("public " + postmod_type + " " + name + ";");
                    }
                }}
            )+
            {{
                System.Console.WriteLine("}");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this struct.
                    System.Console.WriteLine("}");
                    PiggyRuntime.Tool.Redirect.Dispose();
                    PiggyRuntime.Tool.Redirect = null;
                    ClangSupport.FormatFile(generated_file_name);
                }
            }}
        )

        ( RecordDecl SrcRange=$"{ClangSupport.limit}" KindName=* Name=$"{ClangSupport.generate_for_only}" Attrs="definition"
            {{
                string name = tree.Attr("Name");
                string preferred_name = ClangSupport.RewriteAppliedOccurrence(false, name);
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this struct.
                    generated_file_name = PiggyRuntime.Tool.MakeFileNameUnique(PiggyRuntime.Tool.OutputLocation + "g-" + preferred_name + ".cs");
                    PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(generated_file_name);
                    System.Console.WriteLine("// This file generated by Piggy. Do not edit.");
                    System.Console.WriteLine("namespace " + ClangSupport.namespace_name);
                    System.Console.WriteLine("{");
                    System.Console.WriteLine("using System;");
                    System.Console.WriteLine("using System.Runtime.InteropServices;");
                }
                var scope = _stack.Peek();
                var layout = tree.Attr("KindName") == "struct"
                    ? @"[StructLayout(LayoutKind.Sequential)]"
                    : @"[StructLayout(LayoutKind.Explicit)]";
                offset = 0;
                System.Console.WriteLine(
                    layout + @"
                    public partial struct " + preferred_name + @"
                    {");
            }}
            ( FieldDecl
                {{
                    var name = tree.Attr("Name");
                    var premod_type = tree.Attr("Type");
                    Regex regex = new Regex("(?<basetype>.*)[[](?<index>.*)[]]");
                    var match = regex.Match(premod_type);
                    if (match.Success)
                    {
                        var ssize = (string)match.Groups["index"].Value;
                        if (ssize == "")
                        {
                            System.Console.WriteLine("public " + "IntPtr" + " gen" + generated++ + ";");
                        }
                        else 
                        {
                            var num = Int32.Parse(ssize);
                            var basetype = (string)match.Groups["basetype"].Value;
                            var base_postmod_type = ClangSupport.RewriteAppliedOccurrence(false, basetype);
                            for (int i = 0; i < num; ++i)
                            {
                                System.Console.WriteLine("public " + base_postmod_type + " gen" + generated++ + ";");
                            }
                        }
                    }
                    else
                    {
                        var postmod_type = ClangSupport.RewriteAppliedOccurrence(false, premod_type);
                        if (tree.Peek(1).Attr("KindName") == "union")
                            System.Console.WriteLine(@"[FieldOffset(" + offset + ")]");
                        System.Console.WriteLine("public " + postmod_type + " " + name + ";");
                    }
                }}
            )+
            {{
                System.Console.WriteLine("}");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this struct.
                    System.Console.WriteLine("}");
                    PiggyRuntime.Tool.Redirect.Dispose();
                    PiggyRuntime.Tool.Redirect = null;
                    ClangSupport.FormatFile(generated_file_name);
                }
            }}
        )

        // If no fields, make a struct for storing a pointer to the struct.
        ( CXXRecordDecl SrcRange=$"{ClangSupport.limit}" KindName=* Name=$"{ClangSupport.generate_for_only}" Attrs="definition"
            {{
                string name = tree.Attr("Name");
                string preferred_name = ClangSupport.RewriteAppliedOccurrence(false, name);
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    generated_file_name = PiggyRuntime.Tool.MakeFileNameUnique(PiggyRuntime.Tool.OutputLocation + "g-" + preferred_name + ".cs");
                    PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(generated_file_name);
                    System.Console.WriteLine("// This file generated by Piggy. Do not edit.");
                    System.Console.WriteLine("namespace " + ClangSupport.namespace_name);
                    System.Console.WriteLine("{");
                    System.Console.WriteLine("using System;");
                    System.Console.WriteLine("using System.Runtime.InteropServices;");
                }
                var scope = _stack.Peek();
                System.Console.WriteLine(
                    @"[StructLayout(LayoutKind.Sequential)]
                    public partial struct " + preferred_name + @"
                    {
                        public " + preferred_name + @"(IntPtr pointer)
                        {
                            this.Pointer = pointer;
                        }
                        public IntPtr Pointer;
                    }");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this struct.
                    System.Console.WriteLine("}");
                    PiggyRuntime.Tool.Redirect.Dispose();
                    PiggyRuntime.Tool.Redirect = null;
                    ClangSupport.FormatFile(generated_file_name);
                }
            }}
        )
        ( RecordDecl SrcRange=$"{ClangSupport.limit}" KindName=* Name=$"{ClangSupport.generate_for_only}" Attrs="definition"
            {{
                string name = tree.Attr("Name");
                string preferred_name = ClangSupport.RewriteAppliedOccurrence(false, name);
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this struct.
                    generated_file_name = PiggyRuntime.Tool.MakeFileNameUnique(PiggyRuntime.Tool.OutputLocation + "g-" + preferred_name + ".cs");
                    PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(generated_file_name);
                    System.Console.WriteLine("// This file generated by Piggy. Do not edit.");
                    System.Console.WriteLine("namespace " + ClangSupport.namespace_name);
                    System.Console.WriteLine("{");
                    System.Console.WriteLine("using System;");
                    System.Console.WriteLine("using System.Runtime.InteropServices;");
                }
                var scope = _stack.Peek();
                System.Console.WriteLine(
                    @"[StructLayout(LayoutKind.Sequential)]
                    public partial struct " + preferred_name + @"
                    {
                        public " + preferred_name + @"(IntPtr pointer)
                        {
                            this.Pointer = pointer;
                        }
                        public IntPtr Pointer;
                    }");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this struct.
                    System.Console.WriteLine("}");
                    PiggyRuntime.Tool.Redirect.Dispose();
                    PiggyRuntime.Tool.Redirect = null;
                    ClangSupport.FormatFile(generated_file_name);
                }
            }}
        )
    }
}