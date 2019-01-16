using 'Enums.pig';
using 'Structs.pig';
using 'Funcs.pig';
using 'Namespace.pig';
using 'Typedefs.pig';

template Project1Namespace : Namespace
{
	init {{
		namespace_name = "Csharp";
		PiggyRuntime.TemplateHelpers.ModParamUsageType(
			new Dictionary<string, string>() {
			{ "const char **", "out IntPtr" },
			{ "char *", "[Out] byte[]"},
			{ "unsigned int *", "out uint" },
			{ "void **", "out IntPtr" },
			{ "void *", "IntPtr" },
			{ "const char *", "string" },
			{ "const void *", "IntPtr" },
			{ "const <type> *", "in <type>"},
		});
		PiggyRuntime.TemplateHelpers.ModNonParamUsageType(
			new Dictionary<string, string>() {
			{ "char *", "byte[]"},
			{ "size_t", "SizeT" },
			{ "int", "int"},
			{ "uint", "uint"},
			{ "short", "short"},
			{ "ushort", "ushort"},
			{ "long", "long"},
			{ "unsigned char", "byte" },
			{ "unsigned short", "UInt16"},
			{ "unsigned int", "uint"},
			{ "unsigned long", "ulong"},
			{ "unsigned long long", "ulong"},
			{ "long long", "long"},
			{ "float", "float"},
			{ "double", "double"},
			{ "bool", "bool"},
			{ "char", "byte"},
			{ "const char *", "string" },
			{ "signed char", "sbyte" },
		});
	}}
}

template Project1Enums : Enums
{
	init {{
		// Override limits in matching.
		limit = "src";
	}}
}

template Project1Structs : Structs
{
	init {{
		// Override limits in matching.
		limit = "src";
	}}
}

template Project1Typedefs : Typedefs
{
	init {{
		// Override limits in matching.
		limit = "src";
	}}
}


template Project1Funcs : Funcs
{
	init {{
		dllname = "Leptonica";
		// Override limits in matching.
		limit = "src";
	}}
}

application
   Project1Namespace.GenerateStart
   Project1Enums.GenerateEnums
   Project1Typedefs.GeneratePointerTypes
   Project1Structs.GenerateStructs
   Project1Typedefs.GenerateTypedefs
   Project1Funcs.Start
   Project1Funcs.Functions
   Project1Funcs.End
   Project1Namespace.GenerateEnd
   ;
