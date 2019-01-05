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
			{ "CUresult:enum cudaError_enum", "CUresult" }
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
