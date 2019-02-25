
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

    pass Start
    {
        ( body
            ( TOKEN t=";"
            [[ {};]]
        )   )

		( TOKEN t="<EOF>"
            {{
                var i = tree.Attr("i");
                var ii = Int32.Parse(i);
                System.Console.Write(PiggyRuntime.AstHelpers.GetLeftOfToken(ii));
            }}
		)

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
    Serialize.Start
    ;
