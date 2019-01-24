
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
            }}
        )
    }

    pass GenerateTypedefs {
        ( TypedefDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}" ( BuiltinType BareType=*
            {{
                var scope = _stack.Peek();
                var name = tree.Peek(1).Attr("Name");
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
            }}
        ))
    
        ( TypedefDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}" ( ElaboratedType ( RecordType ( CXXRecord Name=*
            {{
                var scope = _stack.Peek();
                var name = tree.Peek(3).Attr("Name");
                var cxxrec_name = tree.Attr("Name");
                cxxrec_name = ClangSupport.ModNonParamUsageType(cxxrec_name);
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
            }}
        ))))

        ( TypedefDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}" ( ElaboratedType ( RecordType ( Record Name=*
            {{
                var scope = _stack.Peek();
                var name = tree.Peek(3).Attr("Name");
                var cxxrec_name = tree.Attr("Name");
                cxxrec_name = ClangSupport.ModNonParamUsageType(cxxrec_name);
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
            }}
        ))))

        ( TypedefDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}" ( ElaboratedType ( EnumType ( Enum Name=*
            {{
                var scope = _stack.Peek();
                var name = tree.Peek(3).Attr("Name");
                var base_name = tree.Attr("Name");
                base_name = ClangSupport.ModNonParamUsageType(base_name);
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
            }}
        ))))
    }
}