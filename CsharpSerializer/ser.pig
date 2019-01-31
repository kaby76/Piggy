
template Serialize
{
	pass Open
	{
		( compilation_unit 
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

        ( TOKEN 
            {{
				var i = tree.Attr("i");
				var ii = Int32.Parse(i);
				System.Console.Write(PiggyRuntime.AstHelpers.GetLeftOfToken(ii));
                var t = tree.Attr("t");
				System.Console.Write(t);
            }}
        )
    }
}

application
	Serialize.Open
    Serialize.Start
    ;
