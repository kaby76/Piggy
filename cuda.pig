using 'Enums.pig';
using 'Structs.pig';

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
    CudaEnums.GenerateHeader
    CudaEnums.GenerateEnums
    CudaStructs.CollectStructs
    CudaStructs.GenerateStructs
    CudaEnums.Functions
    CudaEnums.GenerateEnd
    ;
