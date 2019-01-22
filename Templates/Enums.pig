
template Enums
{
    header {{
        protected bool first = true;
    }}

    pass GenerateEnums {
        ( EnumDecl SrcRange=$"{ClangSupport.limit}" Name=$"{ClangSupport.generate_for_only}"
            {{
                first = true;
                string name = tree.Attr("Name");
                result.Append("public enum " + name + " {" + Environment.NewLine);
            }}
            (%
                ( EnumConstantDecl
                    ( IntegerLiteral
                        {{
                            if (first)
                                first = false;
                            else
                                result.Append("," + Environment.NewLine);
                            result.Append("" + tree.Peek(1).Attr("Name") + " = " + tree.Peek(0).Attr("Value"));
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
                        result.Append("" + tree.Attr("Name"));
                    }}
                )
            %)*
            [[}
            ]]
        )
    }
}