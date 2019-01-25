template Structs
{
    header {{
        protected bool first = true;
        int generated = 0;
        int offset;
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
				if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
				{
					// Create a new file for this struct.

				}

                string name = tree.Attr("Name");
                var scope = _stack.Peek();
                var typedef_name = name;
                var layout = tree.Attr("KindName") == "struct"
                    ? @"[StructLayout(LayoutKind.Sequential)]"
                    : @"[StructLayout(LayoutKind.Explicit)]";
                offset = 0;
                System.Console.WriteLine(
                    layout + @"
                    public partial struct " + name + @"
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
                                var base_postmod_type = ClangSupport.ModNonParamUsageType(basetype);
                                for (int i = 0; i < num; ++i)
                                {
                                    System.Console.WriteLine("public " + base_postmod_type + " gen" + generated++ + ";");
                                }
                            }
                        }
                        else
                        {
                            var postmod_type = ClangSupport.ModNonParamUsageType(premod_type);
                            if (tree.Peek(1).Attr("KindName") == "union")
                                System.Console.WriteLine(@"[FieldOffset(" + offset + ")]");
                            System.Console.WriteLine("public " + postmod_type + " " + name + ";");
                        }
                    }}
                )+
            [[}
            ]]
        )

        ( RecordDecl SrcRange=$"{ClangSupport.limit}" KindName=* Name=$"{ClangSupport.generate_for_only}" Attrs="definition"
            {{
				if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
				{
					// Create a new file for this struct.

				}

                string name = tree.Attr("Name");
                var scope = _stack.Peek();
                var typedef_name = name;
                var layout = tree.Attr("KindName") == "struct"
                    ? @"[StructLayout(LayoutKind.Sequential)]"
                    : @"[StructLayout(LayoutKind.Explicit)]";
                offset = 0;
                System.Console.WriteLine(
                    layout + @"
                    public partial struct " + name + @"
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
                                var base_postmod_type = ClangSupport.ModNonParamUsageType(basetype);
                                for (int i = 0; i < num; ++i)
                                {
                                    System.Console.WriteLine("public " + base_postmod_type + " gen" + generated++ + ";");
                                }
                            }
                        }
                        else
                        {
                            var postmod_type = ClangSupport.ModNonParamUsageType(premod_type);
                            if (tree.Peek(1).Attr("KindName") == "union")
                                System.Console.WriteLine(@"[FieldOffset(" + offset + ")]");
                            System.Console.WriteLine("public " + postmod_type + " " + name + ";");
                        }
                    }}
                )+
            [[}
            ]]
        )

        // If no fields, make a struct for storing a pointer to the struct.
        ( CXXRecordDecl SrcRange=$"{ClangSupport.limit}" KindName=* Name=$"{ClangSupport.generate_for_only}" Attrs="definition"
            {{
				if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
				{
					// Create a new file for this struct.

				}

                string name = tree.Attr("Name");
                var scope = _stack.Peek();
                var typedef_name = name;
                System.Console.WriteLine(
                    @"[StructLayout(LayoutKind.Sequential)]
                    public partial struct " + name + @"
                    {
                        public " + name + @"(IntPtr pointer)
                        {
                            this.Pointer = pointer;
                        }
                        public IntPtr Pointer;
                    }
                    ");
            }}
        )
        ( RecordDecl SrcRange=$"{ClangSupport.limit}" KindName=* Name=$"{ClangSupport.generate_for_only}" Attrs="definition"
            {{
				if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
				{
					// Create a new file for this struct.

				}

                string name = tree.Attr("Name");
                var scope = _stack.Peek();
                var typedef_name = name;
                System.Console.WriteLine(
                    @"[StructLayout(LayoutKind.Sequential)]
                    public partial struct " + name + @"
                    {
                        public " + name + @"(IntPtr pointer)
                        {
                            this.Pointer = pointer;
                        }
                        public IntPtr Pointer;
                    }
                    ");
            }}
        )
    }
}