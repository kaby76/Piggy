using 'basic.pig';

template ClangEnums : Enums
{
	init {{
		// Override limits in matching.
		limit = ".*\\clang-c\\.*";
	}}
}

application
	ClangEnums.GenerateHeader
	ClangEnums.GenerateEnums
	ClangEnums.CollectReturns
	ClangEnums.GenerateReturns
	ClangEnums.Functions
	ClangEnums.GenerateEnd
	;
