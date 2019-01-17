using 'ClangSupport.pig';
using 'Enums.pig';
using 'Structs.pig';
using 'Funcs.pig';
using 'Namespace.pig';
using 'Typedefs.pig';

template Project1Namespace : Namespace
{
	init {{
		namespace_name = "Csharp";
	}}
}

template Project1Enums : Enums
{
	init {{
		// Override limits in matching.
		limit = "[Ss]rc";
	}}
}

template Project1Structs : Structs
{
	init {{
		// Override limits in matching.
		limit = "[Ss]rc";
	}}
}

template Project1Typedefs : Typedefs
{
	init {{
		// Override limits in matching.
		limit = "[Ss]rc";
	}}
}


template Project1Funcs : Funcs
{
	init {{
		dllname = "leptonica-1.77.0d";
		// Override limits in matching.
		limit = "[Ss]rc";
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
