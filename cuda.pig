using 'Enums.pig';
using 'Structs.pig';
using 'Funcs.pig';
using 'Namespace.pig';

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
		generate_for_these = new List<string>() {
			"CUcontext",
			"CUmodule",
			"CUfunction",
			"CUstream",
			"CUlinkState"
		};
		do_not_match_these = new List<string>() {
			"CUstreamCallback",
			"CUstreamMemOpFlushRemoteWritesParams_st"
		};
    }}
}

template CudaFuncs : Funcs
{
    init {{
        limit = ".*\\.*GPU.*\\.*";
		dllname = "nvcuda";
		_parm_type_map = new Dictionary<string, string>() {
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
		};
    }}
}

application
	Namespace.GenerateStart
    CudaEnums.CollectTypedefEnums
	CudaEnums.GenerateEnums
    CudaStructs.CollectStructs
    CudaStructs.GenerateStructs
    CudaFuncs.Functions
    Namespace.GenerateEnd
    ;
