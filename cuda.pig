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
			{ "CUresult:enum cudaError_enum", "CUresult" },
			{ "const char **", "out IntPtr" },
			{ "unsigned int", "uint" },
			{ "int *", "out int" },
			{ "CUdevice *", "out CUdevice" },
			{ "char *", "[Out] byte[]"},
			{ "CUuuid *", "out CUuuid" },
			{ "size_t *", "out SizeT" },
			{ "unsigned int *", "out uint" },
			{ "CUdevprop *", "out CUdevprop" },
			{ "CUcontext *", "out CUcontext" },
			{ "CUfunc_cache *", "out CUfunc_cache" },
			{ "CUmodule *", "out CUmodule" },
			{ "CUsharedconfig *", "out CUsharedconfig" },
			{ "CUfunction *", "out CUfunction" },
			{ "CUdeviceptr *", "out CUdeviceptr" },
			{ "CUtexref *", "out CUtexref" },
			{ "CUsurfref *", "out CUsurfref" },
			{ "void **", "out IntPtr" },
		});
		PiggyRuntime.TemplateHelpers.ModNonParamUsageType(
			new Dictionary<string, string>() {
			{ "char *", "byte[]"},
			{ "const char **", "IntPtr" },
			{ "CUcontext", "CUcontext" },
			{ "CUdevice", "CUdevice" },
			{ "CUdeviceptr", "CUdeviceptr" },
			{ "CUdevprop", "CUdevprop" },
			{ "CUfunc_cache", "CUfunc_cache" },
			{ "CUfunction", "CUfunction" },
			{ "CUmodule", "CUmodule" },
			{ "CUresult:enum cudaError_enum", "CUresult" },
			{ "CUsharedconfig", "CUsharedconfig" },
			{ "CUsurfref", "CUsurfref" },
			{ "CUtexref", "CUtexref" },
			{ "CUuuid", "CUuuid" },
			{ "int *", "IntPtr" },
			{ "size_t", "SizeT" },
			{ "unsigned int", "uint" },
			{ "void **", "IntPtr" },
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
