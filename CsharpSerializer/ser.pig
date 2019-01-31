
template Serialize : Template
{
    pass Start
    {
        ( body
            ( TOKEN t=";"
            [[ {};]]
        )   )

        ( TOKEN 
            {{
                IParseTree t = tree.Current;
                PiggyRuntime.AstHelpers.Reconstruct(t, tree.CommonTokenStream);
            }}
        )
    }
}

application
    Serialize.Start
    ;
