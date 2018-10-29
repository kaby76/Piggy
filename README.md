# Piggy

Welcome to Piggy (*P*/*I*nvoke *G*enerator for C#). This generator is an entirely new way
of generating P/Invoke bindings for C# from C++ headers.
It uses Clang and output templates with embedded C# code
to specify what is the output of the generator.

This code would not be possible if I could not read
the code of [ClangSharp](https://github.com/Microsoft/ClangSharp)
[(Mukul Sabharwal; mjsabby)](https://github.com/mjsabby),
[CppSharp](https://github.com/mono/CppSharp), and example code
to compile and run C# source on on the fly by [Lumír Kojecký](https://www.codeproject.com/script/Membership/View.aspx?mid=9709944)
in [CodeProject](https://www.codeproject.com/Tips/715891/Compiling-Csharp-Code-at-Runtime).

This code does not use AST Visitors, nor does it use the Clang-c interface. It uses
libclang's AST matchers. It statically
links to a standard fully-built version of llvm with clang and clang extras
(see below for details). This generator uses libclang directly,
and Roslyn, the Microsoft C# compiler.

This tool does not read DLLs for P/Invoke generation, only the headers.

Piggy differs from ClangSharp and CppSharp in the following ways:

* Input into Piggy is a specification file that tells the generator what
C/C++ header files to read, what compiler options are used in Clang, type maps
to resolve problems with function parameters that use pointers, and calling
conventions.

## Piggy Specification File

Instead of using and extending the unwieldy command-line arguments for input,
Piggy uses a specification file. This file specifies what options to use
for Clang and C# code generation. The grammar and parser for the specification file is for Antlr, a
high quality parser generator.

### Spec file grammar

#### SpecParser.g4

    Grammar Specparser;

    Options { Tokenvocab = Speclexer; }

    Spec : Items* Eof ;

    Items
        : Namespace
        | Exclude
        | Import_File
        | Dllimport
        | Add_After_Usings
        | Prefix_Strip
        | Class_Name
        | Calling_Convention
        | Compiler_Option
        ;

    /* Specifies A Namespace Name For The Generated C# Code.
     * Example:
     *   Namespace Cuda_10_0_130;
     * 
     *   The Code Generator Will Output:
     *      Namespace Cuda_10_0_130
     *      {
     *        Using System;
     *        Using System.Runtime.Interopservices;
     *        ...
     *      }
     */
    Namespace: Namespace Id Semi ;

    /* Specifies What Function To Avoid Translating. In Other Words, Do Not Generate
     * P/Invoke Code For The Execluded Function. Use Multiple Times To Execlude More Than
     * One Function; Do Not Use Comma Separated List.
     * Example:
     *   Exclude __Internal_Float2Half;
     *   Without The Option, The Generator Would Produce:
     *
     */
    Exclude: Exclude Id Semi ;

    /* Specifies An Input File For The Clang Compiler. Use Forward Slashes For Directory
     * Delimiters.
     * Example:
     *   Import_File 'C:/Program Files/Nvidia Gpu Computing Toolkit/Cuda/V10.0/Include/Cuda.H';
     */
    Import_File: Import_File Stringliteral Semi ;


    /* Specifies A Name For The Dll To Use In P/Invoke Calls In The Generated C# Code.
     * It Is Highly Recommended That Your Dll Does Not Contain The Suffix ".Dll", And Does
     * Not Specify A Path.
     * Example:
     *   Dllimport 'Nvcuda';
     * 
     *   The Code Generator Will Output:
     *      Namespace ...
     *      {
     *        Using System;
     *        Using System.Runtime.Interopservices;
     *        ...
     *          Private Const String Librarypath = "Nvcuda";
     *        ...
     *      }
     */
    Dllimport: Dllimport Stringliteral Semi ;

    /* Specifies Code To Be Included After The "Usings" In The Generated C# Code.
     * The Code Is Everything Between Curly Braces (Which May Be Any C# Code Containing
     * Curly Braces).
     * Example:
     *   Add_After_Usings {
     *       Using Helpers;
     *   };
     * 
     *   The Code Generator Will Output:
     *      Namespace ...
     *      {
     *        Using System;
     *        Using System.Runtime.Interopservices;
     *        Using Helpers;
     *        ...
     *      }
     */
    Add_After_Usings: Add_After_Usings Code Semi ;
    Code: Lcurly Other* Rcurly ;

    /* Specifies A String To Strip From Method And Type Names In The Generated C# Code.
     * Example:
     *   Prefix_Strip Cu;
     *   If The Tool Reads Cuda.H, The Generator Will Remove "Cu" From The Beginning Of Functions.
     *   The Code Generator Will Output:
     *      Namespace ...
     *      {
     *        Using System;
     *        Using System.Runtime.Interopservices;
     *        ...
     *        [Dllimport(Librarypath, Entrypoint = "Cugeterrorstring", Callingconvention = Callingconvention.Stdcall)]
     *        Public Static Extern Cudaerror_Enum Geterrorstring(Cudaerror_Enum @Error, Out Intptr @Pstr);
     *        ...
     *      }
     */
    Prefix_Strip: Prefix_Strip Id Semi ;

    /* Specifies The Name Of The Class Containing The P/Invoke Api In The Generated C# Code.
     * It Is Required.
     * Example:
     *   Class_Name Cuda;
     *   If The Tool Reads Cuda.H, The Generator Will Remove "Cu" From The Beginning Of Functions.
     *   The Code Generator Will Output:
     *      Namespace ...
     *      {
     *        Using System;
     *        Using System.Runtime.Interopservices;
     *        ...
     *            Public Static Partial Class Cuda
     *            {
     *            ...
     *            }
     *      }
     */
    Class_Name: Class_Name Id Semi ;

    /* Specifies The Calling Convention Of The P/Invoke Api In The Generated C# Code.
     * If Not Specified, It Defaults To Cdecl. There Is Code To Test Individual Functions But
     * It Doesn'T Seem To Work In All Cases. Thus This Option.
     * Example:
     *   Calling_Convention Callingconvention.Stdcall;
     *   The Code Generator Will Output:
     *      Namespace ...
     *      {
     *        Using System;
     *        Using System.Runtime.Interopservices;
     *        ...
     *        [Dllimport(Librarypath, Entrypoint = "Cugeterrorstring", Callingconvention = Callingconvention.Stdcall)]
     *        Public Static Extern Cudaerror_Enum Cugeterrorstring(Cudaerror_Enum @Error, Out Intptr @Pstr);
     *        ...
     *      }
     */

    Calling_Convention: Calling_Convention Id Semi ;

    /* Specifies An Additional Clang Compiler Option. Use Forward Slashes For Directory
     * Delimiters. Use Multiple Times To Specify More Than One Option.
     * Example:
     *   Compiler_Options '--Target=X86_64';
     *   Compiler_Options '-Ic:/Program Files/Nvidia Gpu Computing Toolkit/Cuda/V10.0/Include';
     */
    Compiler_Option: Compiler_Option Stringliteral Semi ;

#### SpecLexer.g4

    lexer grammar SpecLexer;
    channels { COMMENTS_CHANNEL }

    SINGLE_LINE_DOC_COMMENT: '///' InputCharacter*    -> channel(COMMENTS_CHANNEL);
    DELIMITED_DOC_COMMENT:   '/**' .*? '*/'           -> channel(COMMENTS_CHANNEL);
    SINGLE_LINE_COMMENT:     '//'  InputCharacter*    -> channel(COMMENTS_CHANNEL);
    DELIMITED_COMMENT:       '/*'  .*? '*/'           -> channel(COMMENTS_CHANNEL);

    CALLING_CONVENTION :    'calling_convention';
    ADD_AFTER_USINGS:   'add_after_usings';
    CLASS_NAME  :   'class_name';
    CODE        :   'code';
    COMPILER_OPTION:    'compiler_option';
    DLLIMPORT   :   'dllimport';
    EXCLUDE     :   'exclude';
    IMPORT_FILE :   'import_file';
    NAMESPACE   :   'namespace';
    PREFIX_STRIP    :   'prefix_strip';
    SEMI        :   ';';
    LCURLY      :   '{' -> pushMode(CODE_0);
    StringLiteral   :   '\'' ( Escape | ~('\'' | '\\' | '\n' | '\r') ) + '\'';
    ID      :   [a-zA-Z_1234567890.]+ ;

    fragment InputCharacter:       ~[\r\n\u0085\u2028\u2029];
    fragment Escape : '\\' ( '\'' | '\\' );
    WS:    [ \t\r\n] -> skip;

    mode CODE_0;

    CODE_0_LCURLY: '{' -> type(OTHER), pushMode(CODE_N);
    RCURLY: '}' -> popMode;     // Close for LCURLY
    CODE_0_OTHER: ~[{}]+ -> type(OTHER);

    mode CODE_N;

    CODE_N_LCURLY: '{' -> type(OTHER), pushMode(CODE_N);
    CODE_N_RCURLY: '}' -> type(OTHER), popMode;
    OTHER: ~[{}]+;


## Building Piggy ##


Download llvm, clang, and clang extra. Build.

Open Visual Studio 2017 on Piggy.sln and build.


