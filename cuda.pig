using 'Enums.pig';
using 'Structs.pig';
using 'Funcs.pig';
using 'Namespace.pig';
using 'Typedefs.pig';

template CudaNamespace : Namespace
{
	init {{
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
	});
	}}
}

template CudaEnums : Enums
{
    init {{
        // Override limits in matching.
        limit = ".*\\.*GPU.*\\.*";
    }}
}

template CudaStructs : Structs
{
    init {{
        limit = ".*\\.*GPU.*\\.*";
		do_not_match_these = new List<string>() {
			"CUstreamCallback",
			"CUstreamMemOpFlushRemoteWritesParams_st"
		};
    }}
}

template CudaTypedefs : Typedefs
{
    init {{
        // Override limits in matching.
        limit = ".*\\.*GPU.*\\.*";
    }}
}


template CudaFuncs : Funcs
{
    init {{
        limit = ".*\\.*GPU.*\\.*";
		dllname = "nvcuda";
    }}

	pass Functions {
        ( FunctionDecl SrcRange=$"{CudaFuncs.limit}" Name="cuModuleLoadDataEx"
            {{ int x = 1; }}
			[[ [DllImport("nvcuda", CallingConvention = CallingConvention.ThisCall, EntryPoint = "cuModuleLoadDataEx")]
			public static extern CUresult cuModuleLoadDataEx(out CUmodule jarg1, IntPtr jarg2, uint jarg3, CUjit_option[] jarg4, IntPtr jarg5);
			
			]]
        )
    }
}

application
	CudaNamespace.GenerateStart
    CudaEnums.CollectTypedefEnums
	CudaEnums.GenerateEnums
    CudaTypedefs.GeneratePointerTypes
    CudaStructs.GenerateStructs
	CudaTypedefs.GenerateTypedefs
	CudaFuncs.Start
    CudaFuncs.Functions
	CudaFuncs.End
    Namespace.GenerateEnd
    ;
