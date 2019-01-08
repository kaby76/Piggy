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
		var list = new List<string>() {
			"cudaError_enum",
			"CUdevice_attribute_enum",
			"CUjit_option_enum",
			"CUmemAttach_flags_enum",
			"CUjitInputType_enum",
			};
		generate_for_only = String.Join("|", list);
    }}
}

template CudaStructs : Structs
{
    init {{
        // Override limits in matching.
        limit = ".*\\.*GPU.*\\.*";
		var list = new List<string>() {
			"xxxxxx",
			};
		generate_for_only = String.Join("|", list);
    }}
}

template CudaTypedefs : Typedefs
{
    init {{
        // Override limits in matching.
        limit = ".*\\.*GPU.*\\.*";
		var list = new List<string>() {
			"^CUresult$",
			"^CUcontext$",
			"^CUfunction$",
			"^CUlinkState$",
			"^CUmodule$",
			"^CUstream$",
			"^CUdevice$",
			"^CUjit_option$",
			"^CUdeviceptr$",
			};
		generate_for_only = String.Join("|", list);
    }}
}


template CudaFuncs : Funcs
{
    init {{
        limit = ".*\\.*GPU.*\\.*";
		var list = new List<string>() {
			"^cuCtxSynchronize$",
			"^cuDeviceGet$",
			"^cuDeviceGetCount$",
			"^cuDeviceGetName$",
			"^cuDevicePrimaryCtxReset$",
			"^cuDeviceTotalMem_v2$",
			"^cuGetErrorString$",
			"^cuInit$",
			"^cuLaunchKernel$",
			"^cuLinkComplete$",
			"^cuMemFreeHost$",
			"^cuMemGetInfo_v2$",
			"^cuModuleGetGlobal_v2$" };
		generate_for_only = String.Join("|", list);
		dllname = "nvcuda";
    }}

	pass Functions {
        ( FunctionDecl SrcRange=$"{CudaFuncs.limit}" Name="cuModuleLoadDataEx"
			[[ [DllImport("nvcuda", CallingConvention = CallingConvention.ThisCall, EntryPoint = "cuModuleLoadDataEx")]
			public static extern CUresult cuModuleLoadDataEx(out CUmodule jarg1, IntPtr jarg2, uint jarg3, CUjit_option[] jarg4, IntPtr jarg5);
			
			]]
        )
    }
}

application
	CudaNamespace.GenerateStart
	CudaEnums.GenerateEnums
    CudaTypedefs.GeneratePointerTypes
    CudaStructs.GenerateStructs
	CudaTypedefs.GenerateTypedefs
	CudaFuncs.Start
    CudaFuncs.Functions
	CudaFuncs.End
    Namespace.GenerateEnd
    ;
