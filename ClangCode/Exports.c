
#if defined(_MSC_VER)
//  Microsoft 
#define EXPORT __declspec(dllexport)
#define IMPORT __declspec(dllimport)
#elif defined(__GNUC__)
//  GCC
#define EXPORT __attribute__((visibility("default")))
#define IMPORT
#else
//  do nothing and hope for the best?
#define EXPORT
#define IMPORT
#pragma warning Unknown dynamic link import/export semantics.
#endif

#ifndef STDCALL
# if defined(_MSC_VER)
#   define STDCALL __stdcall
# else
#   define STDCALL
# endif
#endif


extern void Internal_ClangAddOption(char * i);

EXPORT void ClangAddOption(char * i)
{
	Internal_ClangAddOption(i);
}

extern void Internal_ClangAddFile(char * i);

EXPORT void ClangAddFile(char * i)
{
	Internal_ClangAddFile(i);
}

extern char * Internal_ClangSerializeAst();

EXPORT char * ClangSerializeAst()
{
	return Internal_ClangSerializeAst();
}

extern void Internal_ClangSetPackedAst(int packed_ast);

EXPORT void ClangSetPackedAst(int packed_ast)
{
	Internal_ClangSetPackedAst(packed_ast);
}

extern char * Internal_Name(void * p);

EXPORT char * Name(void * p)
{
	return Internal_Name(p);
}
