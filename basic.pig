
template Enums
{
    header {{
        protected bool first = true;
        protected List<string> signatures = new List<string>();
        protected string limit = ""; // Context of what file can match.
		protected string dllname = "unknown_dll"; // A dll to load for DllImport.
        protected int counter;
		protected HashSet<string> done = new HashSet<string>();
    }}

    pass GenerateHeader {
        // Generate declarations at start of the file.
        ( TranslationUnitDecl
            [[
            // ----------------------------------------------------------------------------
            // This is autogenerated code by Piggy.
            // Do not edit this file or all your changes will be lost after re-generation.
            // ----------------------------------------------------------------------------
            using System;
            using System.Runtime.InteropServices;
            using System.Security;

            namespace clangc {
            ]] Pointer=*
        )
    }

    pass GenerateEnums {
        ( EnumDecl SrcRange=$"{Enums.limit}"
            {{
                first = true;
                string name = tree.Attr("Name");
                if (name == "")
                {
                    name = "GeneratedName" + counter++;
                }
                result.Append("public enum @" + name + " {" + Environment.NewLine);
            }}
            (%
                ( EnumConstantDecl
                    ( IntegerLiteral
                        {{
                            if (first)
                                first = false;
                            else
                                result.Append("," + Environment.NewLine);
                            result.Append("@" + tree.Peek(1).Attr("Name") + " = " + tree.Peek(0).Attr("Value"));
                        }}
                    )
                )
                |
                ( EnumConstantDecl
                    {{
                        if (first)
                            first = false;
                        else
                            result.Append("," + Environment.NewLine);
                        result.Append("@" + tree.Attr("Name"));
                    }}
                )
            %)*
            [[}
            ]]
        )
    }

    pass CollectStructs {
        ( FunctionDecl SrcRange=$"{Enums.limit}" Name=*
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

    pass Functions {
        ( FunctionDecl SrcRange=$"{Enums.limit}" Name=*
            {{
                result.Append("[DllImport(" + dllname + ", CallingConvention = CallingConvention.ThisCall,"
                   + " EntryPoint=\"" + tree.Attr("Name") + "\")]" + Environment.NewLine);
                result.Append("public static extern "
                   + PiggyRuntime.TemplateHelpers.GetFunctionReturn(tree.Attr("Type")) + " "
                   + tree.Attr("Name") + "(");
				result.AppendLine("");
                first = true;
            }}
            ( ParmVarDecl Name=* Type=*
                {{
                    if (first)
                        first = false;
                    else
                        result.Append(", ");
                    var premod_type = tree.Attr("Type");
                    var postmod_type = PiggyRuntime.TemplateHelpers.ModParamType(premod_type);
                    result.Append(postmod_type + " " + tree.Attr("Name"));
                }}
            )*
            [[);
            ]]
        )
    }

    pass GenerateEnd {
        ( TranslationUnitDecl
            [[
                }
                // End of translation unit.
            ]]
        )
    }
}