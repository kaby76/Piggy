
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
                System.Console.Write("public enum " + name + " {" + Environment.NewLine);
            }}
            (%
                ( EnumConstantDecl
                    ( IntegerLiteral
                        {{
                            if (first)
                                first = false;
                            else
                                System.Console.Write("," + Environment.NewLine);
                            System.Console.Write("" + tree.Peek(1).Attr("Name") + " = " + tree.Peek(0).Attr("Value"));
                        }}
                    )
                )
                |
                ( EnumConstantDecl
                    {{
                        if (first)
                            first = false;
                        else
                            System.Console.Write("," + Environment.NewLine);
                        System.Console.Write("" + tree.Attr("Name"));
                    }}
                )
            %)*
            [[}
            ]]
        )
    }
}