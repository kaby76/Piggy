using 'ClangSupport.pig';
using 'Enums.pig';
using 'Structs.pig';
using 'Funcs.pig';
using 'Namespace.pig';
using 'Typedefs.pig';

template Project1Support {
	init {{
		ClangSupport.dllname = "leptonica-1.77.0d";
		ClangSupport.namespace_name = "Csharp";
		ClangSupport.limit = "[Ss]rc";
	}}
}

template Project1Namespace : Namespace { }

template Project1Enums : Enums { }

template Project1Structs : Structs { }

template Project1Typedefs : Typedefs { }


template Project1Funcs : Funcs { }

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
