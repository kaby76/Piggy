
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
}