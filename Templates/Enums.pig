
template Enums
{
    header {{
        protected bool first = true;
        protected string generate_for_only = ".*"; // default to everything.
    }}

    pass GenerateEnums {
        ( EnumDecl SrcRange=$"{Enums.limit}" Name=$"{Enums.generate_for_only}"
            {{
                first = true;
                string name = tree.Attr("Name");
//                var scope = _stack.Peek();
//                var typedef_name = scope.resolve(name, true);
//                if (typedef_name != null) name = typedef_name.Name;
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