
template Typedefs
{
    // The only mechanism to create an alias type in C# is to use a struct,
    // where there is one field of the type desired.
    //
    // There are so far the following cases:

    pass GeneratePointerTypes {
        ( TypedefDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}" ( PointerType )
            {{
                var scope = _stack.Peek();
                var name = tree.Attr("Name");
                var baretype_name = "IntPtr";
                var def = scope.getSymbol(name);
                if (def != null) return;

                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    var output_file_name = PiggyRuntime.Tool.OutputLocation + "g-" + name + ".cs";
                    PiggyRuntime.Tool.GeneratedFiles.Add(output_file_name);
                    PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(output_file_name);
                    System.Console.WriteLine("namespace " + ClangSupport.namespace_name);
                    System.Console.WriteLine("{");
                    System.Console.WriteLine("using System;");
                    System.Console.WriteLine("using System.Runtime.InteropServices;");
                }

                def = new StructSymbol(name);
                scope.define(def);
                System.Console.WriteLine(
                    @"[StructLayout(LayoutKind.Sequential)]
                    public partial struct " + name + @"
                    {
                        public " + baretype_name + @" Value;
                        public " + name + @"(" + baretype_name + @" value)
                        {
                            this.Value = value;
                        }
                    }
                    ");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this struct.
                    System.Console.WriteLine("}");
                    PiggyRuntime.Tool.Redirect.Dispose();
                    PiggyRuntime.Tool.Redirect = null;
                    var output_file_name = PiggyRuntime.Tool.OutputLocation + "g-" + name + ".cs";
                    ClangSupport.FormatFile(output_file_name);
                }
            }}
        )
    }

    pass GenerateTypedefs {
        ( TypedefDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}" ( BuiltinType BareType=*
            {{
                var scope = _stack.Peek();
                var name = tree.Peek(1).Attr("Name");

                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    var output_file_name = PiggyRuntime.Tool.OutputLocation + "g-" + name + ".cs";
                    PiggyRuntime.Tool.GeneratedFiles.Add(output_file_name);
                    PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(output_file_name);
                    System.Console.WriteLine("namespace " + ClangSupport.namespace_name);
                    System.Console.WriteLine("{");
                    System.Console.WriteLine("using System;");
                    System.Console.WriteLine("using System.Runtime.InteropServices;");
                }

                var baretype_name = tree.Attr("BareType");
                baretype_name = ClangSupport.ModNonParamUsageType(baretype_name);
                System.Console.WriteLine(
                    @"[StructLayout(LayoutKind.Sequential)]
                    public partial struct " + name + @"
                    {
                        public " + baretype_name + @" Value;
                        public " + name + @"(" + baretype_name + @" value)
                        {
                            this.Value = value;
                        }
                    }
                    ");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this struct.
                    System.Console.WriteLine("}");
                    PiggyRuntime.Tool.Redirect.Dispose();
                    PiggyRuntime.Tool.Redirect = null;
                    var output_file_name = PiggyRuntime.Tool.OutputLocation + "g-" + name + ".cs";
                    ClangSupport.FormatFile(output_file_name);
                }
            }}
        ))
    
        ( TypedefDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}" ( ElaboratedType ( RecordType ( CXXRecord Name=*
            {{
                var scope = _stack.Peek();
                var name = tree.Peek(3).Attr("Name");
                var cxxrec_name = tree.Attr("Name");
                cxxrec_name = ClangSupport.ModNonParamUsageType(cxxrec_name);

                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    var output_file_name = PiggyRuntime.Tool.OutputLocation + "g-" + name + ".cs";
                    PiggyRuntime.Tool.GeneratedFiles.Add(output_file_name);
                    PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(output_file_name);
                    System.Console.WriteLine("namespace " + ClangSupport.namespace_name);
                    System.Console.WriteLine("{");
                    System.Console.WriteLine("using System;");
                    System.Console.WriteLine("using System.Runtime.InteropServices;");
                }

                System.Console.WriteLine(
                    @"[StructLayout(LayoutKind.Sequential)]
                    public partial struct " + name + @"
                    {
                        public " + cxxrec_name + @" Value;
                        public " + name + @"(" + cxxrec_name + @" value)
                        {
                            this.Value = value;
                        }
                    }
                    ");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this struct.
                    System.Console.WriteLine("}");
                    PiggyRuntime.Tool.Redirect.Dispose();
                    PiggyRuntime.Tool.Redirect = null;
                    var output_file_name = PiggyRuntime.Tool.OutputLocation + "g-" + name + ".cs";
                    ClangSupport.FormatFile(output_file_name);
                }
            }}
        ))))

        ( TypedefDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}" ( ElaboratedType ( RecordType ( Record Name=*
            {{
                var scope = _stack.Peek();
                var name = tree.Peek(3).Attr("Name");
                var cxxrec_name = tree.Attr("Name");
                cxxrec_name = ClangSupport.ModNonParamUsageType(cxxrec_name);

                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    var output_file_name = PiggyRuntime.Tool.OutputLocation + "g-" + name + ".cs";
                    PiggyRuntime.Tool.GeneratedFiles.Add(output_file_name);
                    PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(output_file_name);
                    System.Console.WriteLine("namespace " + ClangSupport.namespace_name);
                    System.Console.WriteLine("{");
                    System.Console.WriteLine("using System;");
                    System.Console.WriteLine("using System.Runtime.InteropServices;");
                }

                System.Console.WriteLine(
                    @"[StructLayout(LayoutKind.Sequential)]
                    public partial struct " + name + @"
                    {
                        public " + cxxrec_name + @" Value;
                        public " + name + @"(" + cxxrec_name + @" value)
                        {
                            this.Value = value;
                        }
                    }
                    ");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this struct.
                    System.Console.WriteLine("}");
                    PiggyRuntime.Tool.Redirect.Dispose();
                    PiggyRuntime.Tool.Redirect = null;
                    var output_file_name = PiggyRuntime.Tool.OutputLocation + "g-" + name + ".cs";
                    ClangSupport.FormatFile(output_file_name);
                }
            }}
        ))))

        ( TypedefDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}" ( ElaboratedType ( EnumType ( Enum Name=*
            {{
                var scope = _stack.Peek();
                var name = tree.Peek(3).Attr("Name");
                var base_name = tree.Attr("Name");
                base_name = ClangSupport.ModNonParamUsageType(base_name);

                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    var output_file_name = PiggyRuntime.Tool.OutputLocation + "g-" + name + ".cs";
                    PiggyRuntime.Tool.GeneratedFiles.Add(output_file_name);
                    PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(output_file_name);
                    System.Console.WriteLine("namespace " + ClangSupport.namespace_name);
                    System.Console.WriteLine("{");
                    System.Console.WriteLine("using System;");
                    System.Console.WriteLine("using System.Runtime.InteropServices;");
                }

                System.Console.WriteLine(
                    @"[StructLayout(LayoutKind.Sequential)]
                    public partial struct " + name + @"
                    {
                        public " + base_name + @" Value;
                        public " + name + @"(" + base_name + @" value)
                        {
                            this.Value = value;
                        }
                    }
                    ");
                if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
                {
                    // Create a new file for this struct.
                    System.Console.WriteLine("}");
                    PiggyRuntime.Tool.Redirect.Dispose();
                    PiggyRuntime.Tool.Redirect = null;
                    var output_file_name = PiggyRuntime.Tool.OutputLocation + "g-" + name + ".cs";
                    ClangSupport.FormatFile(output_file_name);
                }
            }}
        ))))
    }
}