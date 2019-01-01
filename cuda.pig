using 'Decls.pig';
using 'Enums.pig';
using 'Structs.pig';

template CudaDecls : Decls
{
    init {{
        limit = ".*\\.*GPU.*\\.*";
    }}
}

template CudaEnums : Enums
{
    init {{
        // Override limits in matching.
        limit = ".*\\.*GPU.*\\.*";
        dllname = "nvcuda";
    }}
}

template CudaStructs : Structs
{
    init {{
        limit = ".*\\.*GPU.*\\.*";
    }}
}

application
    CudaDecls.CollectEnums
    CudaEnums.GenerateHeader
    CudaEnums.GenerateEnums
    CudaStructs.CollectStructs
    CudaStructs.GenerateStructs
    CudaEnums.Functions
    CudaEnums.GenerateEnd
    ;
