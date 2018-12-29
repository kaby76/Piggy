using 'basic.pig';

template ClangEnums : Enums
{
	init {{
		// Override limits in matching.
		limit = ".*\\clang-c\\.*";
		dllname = "libclang";
	}}
}

application
	ClangEnums.GenerateHeader
	ClangEnums.GenerateEnums
	ClangEnums.CollectStructs
	ClangEnums.GenerateStructs
	ClangEnums.Functions
	ClangEnums.GenerateEnd
	;
