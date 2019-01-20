template Structs
{
    header {{
        protected bool first = true;
        protected string generate_for_only = ".*"; // everything.
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
			sb.Append(generate_for_only);
			generate_for_only = sb.ToString();			
		}}
		)
		
		( CXXRecordDecl SrcRange=$"{Structs.limit}" KindName=* Name=$"{Structs.generate_for_only}" Attrs="definition"
            {{
                string name = tree.Attr("Name");
                var scope = _stack.Peek();
                var typedef_name = name;
                var layout = tree.Attr("KindName") == "struct"
                    ? @"[StructLayout(LayoutKind.Sequential)]"
                    : @"[StructLayout(LayoutKind.Explicit)]";
                offset = 0;
                result.AppendLine(
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
                                result.AppendLine("public " + "IntPtr" + " gen" + generated++ + ";");
                            }
                            else 
                            {
                                var num = Int32.Parse(ssize);
                                var basetype = (string)match.Groups["basetype"].Value;
                                var base_postmod_type = ClangSupport.ModNonParamUsageType(basetype);
                                for (int i = 0; i < num; ++i)
                                {
                                    result.AppendLine("public " + base_postmod_type + " gen" + generated++ + ";");
                                }
                            }
                        }
                        else
                        {
                            var postmod_type = ClangSupport.ModNonParamUsageType(premod_type);
                            if (tree.Peek(1).Attr("KindName") == "union")
                                result.AppendLine(@"[FieldOffset(" + offset + ")]");
                            result.AppendLine("public " + postmod_type + " " + name + ";");
                        }
                    }}
                )+
            [[}
            ]]
        )

        ( RecordDecl SrcRange=$"{Structs.limit}" KindName=* Name=$"{Structs.generate_for_only}" Attrs="definition"
            {{
                string name = tree.Attr("Name");
                var scope = _stack.Peek();
                var typedef_name = name;
                var layout = tree.Attr("KindName") == "struct"
                    ? @"[StructLayout(LayoutKind.Sequential)]"
                    : @"[StructLayout(LayoutKind.Explicit)]";
                offset = 0;
                result.AppendLine(
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
                                result.AppendLine("public " + "IntPtr" + " gen" + generated++ + ";");
                            }
                            else 
                            {
                                var num = Int32.Parse(ssize);
                                var basetype = (string)match.Groups["basetype"].Value;
                                var base_postmod_type = ClangSupport.ModNonParamUsageType(basetype);
                                for (int i = 0; i < num; ++i)
                                {
                                    result.AppendLine("public " + base_postmod_type + " gen" + generated++ + ";");
                                }
                            }
                        }
                        else
                        {
                            var postmod_type = ClangSupport.ModNonParamUsageType(premod_type);
                            if (tree.Peek(1).Attr("KindName") == "union")
                                result.AppendLine(@"[FieldOffset(" + offset + ")]");
                            result.AppendLine("public " + postmod_type + " " + name + ";");
                        }
                    }}
                )+
            [[}
            ]]
        )

        // If no fields, make a struct for storing a pointer to the struct.
        ( CXXRecordDecl SrcRange=$"{Structs.limit}" KindName=* Name=$"{Structs.generate_for_only}" Attrs="definition"
            {{
                string name = tree.Attr("Name");
                var scope = _stack.Peek();
                var typedef_name = name;
                result.AppendLine(
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
        ( RecordDecl SrcRange=$"{Structs.limit}" KindName=* Name=$"{Structs.generate_for_only}" Attrs="definition"
            {{
                string name = tree.Attr("Name");
                var scope = _stack.Peek();
                var typedef_name = name;
                result.AppendLine(
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