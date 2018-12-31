template Structs
{
    header {{
        protected bool first = true;
        protected List<string> signatures = new List<string>();
        protected string limit = ""; // Context of what file can match.
        protected string dllname = "unknown_dll"; // A dll to load for DllImport.
        protected int counter;
        protected HashSet<string> done = new HashSet<string>();
    }}

    pass CollectStructs {
        ( FunctionDecl SrcRange=$"{Structs.limit}" Name=*
            {{
                signatures.Add(tree.Attr("Type"));
            }}
        )
    }

    pass GenerateStructs {
        ( TranslationUnitDecl
            {{
                foreach (var l in signatures)
                {
                    var m = PiggyRuntime.TemplateHelpers.GetFunctionReturn(l);
                    m = m.Trim();
                    var b = PiggyRuntime.TemplateHelpers.BaseType(m);
                    if (!b) continue;
                    if (m == "void") continue;
                    if (done.Contains(m)) continue;
                    done.Add(m);
                    result.AppendLine(
                        @"public partial struct " + m + @"
                        {
                            public " + m + @"(IntPtr pointer)
                            {
                                this.Pointer = pointer;
                            }
                            public IntPtr Pointer;
                        }
                        ");
                }
            }}
        )
    }
}