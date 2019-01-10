template Structs
{
    header {{
        protected bool first = true;
        protected string generate_for_only = ".*"; // everything.
        int generated = 0;
    }}

    pass GenerateStructs {
        // If there are fields, set them up.
        ( CXXRecordDecl SrcRange=$"{Structs.limit}" KindName="struct" Name=$"{Structs.generate_for_only}" Attrs="definition"
            {{
                string name = tree.Attr("Name");
                var scope = _stack.Peek();
//                var typedef_name = scope.resolve(name, true);
                var typedef_name = name;
//                if (typedef_name != null) name = typedef_name.Name;
                result.AppendLine(
                    @"[StructLayout(LayoutKind.Sequential)]
                    public partial struct " + name + @"
                    {");
            }}
                ( FieldDecl
                    {{
                        var name = tree.Attr("Name");
                        var premod_type = tree.Attr("Type");
                        var postmod_type = PiggyRuntime.TemplateHelpers.ModNonParamUsageType(premod_type);
                        Regex regex = new Regex("(?<basetype>.*)[[](?<index>.*)[]]");
                        var match = regex.Match(postmod_type);
                        if (match.Success)
                        {
                            var ssize = (string)match.Groups["index"].Value;
                            var num = Int32.Parse(ssize);
                            var basetype = (string)match.Groups["basetype"].Value;
                            var base_postmod_type = PiggyRuntime.TemplateHelpers.ModNonParamUsageType(basetype);
                            for (int i = 0; i < num; ++i)
                            {
                                result.AppendLine("public " + base_postmod_type + " gen" + generated++ + ";");
                            }
                        }
                        else
                        {
                            result.AppendLine("public " + postmod_type + " " + name + ";");
                        }
                    }}
                )+
            [[}
            ]]
        )

        // If no fields, make a struct for storing a pointer to the struct.
        ( CXXRecordDecl SrcRange=$"{Structs.limit}" KindName="struct" Name=$"{Structs.generate_for_only}" Attrs="definition"
            {{
                string name = tree.Attr("Name");
                var scope = _stack.Peek();
//                var typedef_name = scope.resolve(name, true);
                var typedef_name = name;
//                if (typedef_name != null) name = typedef_name.Name;
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