
template Serialize
{
    pass Open
    {
        ( * File=*
            {{
                var file_name = tree.Attr("File");
                PiggyRuntime.AstHelpers.OpenTokenStream(file_name);
            }}
        )
    }

	pass SetupSymtab
	{
		( classDeclaration
			( TOKEN t="class" )
			( TOKEN t=*
				{{
					var c = new org.antlr.symtab.ClassSymbol(tree.Attr("t"));
					System.Console.Error.WriteLine("Creating class " + c.Name);
				}}
			)
		)
	}

    pass Start
    {
		// For C#.
        ( body
            ( TOKEN t=";"
            [[ {};]]
        )   )

		// Don't print the <EOF> token.
		( TOKEN t="<EOF>"
            {{
                var i = tree.Attr("i");
                var ii = Int32.Parse(i);
                System.Console.Write(PiggyRuntime.AstHelpers.GetLeftOfToken(ii));
            }}
		)

		( classOrInterfaceModifier
            ( annotation
				( TOKEN t=*
					{{
						var i = tree.Attr("i");
						var ii = Int32.Parse(i);
						System.Console.Write(PiggyRuntime.AstHelpers.GetLeftOfToken(ii));
					}}
				)
				( qualifiedName
					( TOKEN t="Override"
						{{
							var i = tree.Attr("i");
							var ii = Int32.Parse(i);
							System.Console.Write(PiggyRuntime.AstHelpers.GetLeftOfToken(ii));
							System.Console.Write("override");
						}}
		)   )	)	)

		// Print out non-token data to the left of the token, and the token itself.
        ( TOKEN 
            {{
                var i = tree.Attr("i");
                var ii = Int32.Parse(i);
                System.Console.Write(PiggyRuntime.AstHelpers.GetLeftOfToken(ii));
                var t = tree.Attr("t");
				// unescape.
				var un_t = Regex.Unescape(t);
                System.Console.Write(un_t);
            }}
        )
    }
}

application
    Serialize.Open
	Serialize.SetupSymtab
    Serialize.Start
    ;
