clang_file 'c:/temp/include/clang-c/Index.h';
clang_option '-IC:/temp/include';

// Include defaults. Order of rules are first come, first served.
using 'basic.pig';

template ClangEnums : Enums
{
	ClangEnums {{
		// Override limits in matching.
		limit = ".*\\clang-c\\.*";
	}}
}
