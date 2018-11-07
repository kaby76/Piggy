# Piggy

Welcome to Piggy (*P*/*I*nvoke *G*enerator for C#). This free and open source software
is a new way
of generating pinvoke bindings for C# from C++ headers, using templates in a powerful
and concise language of tree regular expressions, output strings, and C# code.
Input header files are parsed
by Clang to get an [abstract syntax tree (AST)](http://clang.llvm.org/docs/IntroductionToTheClangAST.html).
Output templates contain tree regular expressions, inlined code, and embedded C# code
for processing of the tree data into output.
This tool does not read DLLs for P/Invoke generation, only the headers.

Piggy extends the ideas of other pinvoke generators:
* [SWIG](http://swig.org/), the original pinvoke generator, which uses a specification file containing type maps.
* [ClangSharp](https://github.com/Microsoft/ClangSharp) [(Mukul Sabharwal; mjsabby)](https://github.com/mjsabby),
 and [CppSharp](https://github.com/mono/CppSharp), which use Clang AST visitors of the Clang-C API.
* [Lumír Kojecký's](https://www.codeproject.com/script/Membership/View.aspx?mid=9709944)
 [CodeProject article for dynamically compiling and executing C# code in the Net Framework](https://www.codeproject.com/Tips/715891/Compiling-Csharp-Code-at-Runtime).
* [Clang-query](https://github.com/llvm-mirror/clang-tools-extra/tree/master/clang-query),
which was used as a starting point for serializing an AST (the XML serializer no longer exists).
* [Jared Parsons' PInvoke Interop Assistant](https://github.com/jaredpar/pinvoke),
which is another open-source pinvoke generator.
* [xInterop C++.Net Bridge, by Shawn Liu](https://www.xinterop.com/). Commercial product, no longer available.

Piggy differs from these projects in a number of ways:

* Input into Piggy is a specification file that tells the generator what
C/C++ header files to read, what compiler options are used in Clang,
(AST matchers, template) pairs that describe what to look for in the AST,
and how to print the value out. I am a strong believer of command-line programs.
A specification file states the requirements for doing the transformation
so you don't have to guess how it was done for generating an API like
Swigged.CUDA.
* Piggy uses Clang ASTs, in libclang, and serializes the tree from code derived
from [ASTDumper.cpp](https://github.com/llvm-mirror/clang/blob/master/lib/AST/ASTDumper.cpp).
It does not use the [Clang-C Visitor API](https://clang.llvm.org/doxygen/group__CINDEX__CURSOR__TRAVERSAL.html)).
* A template defines how to process the AST using tree matching, output strings, and embedded C# code.
* Piggy links to a private, fully-built version of llvm with clang and clang extras.
The user should not be required to install the pre-build LLVM executables (and there
are several critical things missing in the executables).

## Piggy Specification File

Instead of using and extending the unwieldy command-line arguments for input,
Piggy uses a specification file. This file specifies what options to use
for Clang and C# code generation. The grammar and parser for the specification file is for Antlr, a
high quality parser generator.

### Spec file grammar

#### SpecParser.g4

grammar SpecParser;

options { tokenVocab = SpecLexer; }

spec : items* EOF ;

items
    : namespace
    | exclude
    | import_file
    | dllimport
    | add_after_usings
    | prefix_strip
    | class_name
    | calling_convention
    | compiler_option
    | template
    ;

/* Specifies a namespace name for the generated C# code.
 * Example:
 *   namespace Cuda_10_0_130;
 * 
 *   The code generator will output:
 *      namespace Cuda_10_0_130
 *      {
 *        using System;
 *        using System.Runtime.InteropServices;
 *        ...
 *      }
 */
namespace: NAMESPACE ID SEMI ;

/* Specifies what function to avoid translating. In other words, do not generate
 * P/Invoke code for the execluded function. Use multiple times to execlude more than
 * one function; do not use comma separated list.
 * Example:
 *   exclude __internal_float2half;
 *   Without the option, the generator would produce:
 *
 */
exclude: EXCLUDE ID SEMI ;

/* Specifies an input file for the Clang compiler. Use forward slashes for directory
 * delimiters.
 * Example:
 *   import_file 'c:/Program Files/NVIDIA GPU Computing Toolkit/cuda/v10.0/include/cuda.h';
 */
import_file: IMPORT_FILE StringLiteral SEMI ;


/* Specifies a name for the DLL to use in P/Invoke calls in the generated C# code.
 * It is highly recommended that your DLL does not contain the suffix ".dll", and does
 * not specify a path.
 * Example:
 *   dllimport 'nvcuda';
 * 
 *   The code generator will output:
 *      namespace ...
 *      {
 *        using System;
 *        using System.Runtime.InteropServices;
 *        ...
 *          private const string libraryPath = "nvcuda";
 *        ...
 *      }
 */
dllimport: DLLIMPORT StringLiteral SEMI ;

/* Specifies code to be included after the "usings" in the generated C# code.
 * the code is everything between curly braces (which may be any C# code containing
 * curly braces).
 * Example:
 *   add_after_usings {
 *       using Helpers;
 *   };
 * 
 *   The code generator will output:
 *      namespace ...
 *      {
 *        using System;
 *        using System.Runtime.InteropServices;
 *        using Helpers;
 *        ...
 *      }
 */
add_after_usings: ADD_AFTER_USINGS code SEMI ;
code: LCURLY OTHER* RCURLY ;

/* Specifies a string to strip from method and type names in the generated C# code.
 * Example:
 *   prefix_strip cu;
 *   If the tool reads cuda.h, the generator will remove "cu" from the beginning of functions.
 *   The code generator will output:
 *      namespace ...
 *      {
 *        using System;
 *        using System.Runtime.InteropServices;
 *        ...
 *        [DllImport(libraryPath, EntryPoint = "cuGetErrorString", CallingConvention = CallingConvention.StdCall)]
 *        public static extern cudaError_enum GetErrorString(cudaError_enum @error, out IntPtr @pStr);
 *        ...
 *      }
 */
prefix_strip: PREFIX_STRIP ID SEMI ;

/* Specifies the name of the class containing the P/Invoke API in the generated C# code.
 * It is required.
 * Example:
 *   class_name CUDA;
 *   If the tool reads cuda.h, the generator will remove "cu" from the beginning of functions.
 *   The code generator will output:
 *      namespace ...
 *      {
 *        using System;
 *        using System.Runtime.InteropServices;
 *        ...
 *            public static partial class CUDA
 *            {
 *            ...
 *            }
 *      }
 */
class_name: CLASS_NAME ID SEMI ;

/* Specifies the calling convention of the P/Invoke API in the generated C# code.
 * If not specified, it defaults to Cdecl. There is code to test individual functions but
 * it doesn't seem to work in all cases. Thus this option.
 * Example:
 *   calling_convention CallingConvention.StdCall;
 *   The code generator will output:
 *      namespace ...
 *      {
 *        using System;
 *        using System.Runtime.InteropServices;
 *        ...
 *        [DllImport(libraryPath, EntryPoint = "cuGetErrorString", CallingConvention = CallingConvention.StdCall)]
 *        public static extern cudaError_enum cuGetErrorString(cudaError_enum @error, out IntPtr @pStr);
 *        ...
 *      }
 */

calling_convention: CALLING_CONVENTION ID SEMI ;

/* Specifies an additional Clang compiler option. Use forward slashes for directory
 * delimiters. Use multiple times to specify more than one option.
 * Example:
 *   compiler_options '--target=x86_64';
 *   compiler_options '-Ic:/Program Files/NVIDIA GPU Computing Toolkit/cuda/v10.0/include';
 */
compiler_option: COMPILER_OPTION StringLiteral SEMI ;

/* Specifies a template for output. Basic elements of the template:
 *
 *    (% ... %) denotes a tree pattern for AST matching.
 *    < ... > denotes text which is output to the file.
 *    { ... } denotes C# code which is executed after matching the pattern.
 *
 * Tree patterns can be nested, denoting matching of children. Eg, "(% ... (% ... %) %)"
 * matches a node with another node nesting.
 * Text output can be place almost anywhere in a pattern, e.g., "(% ... <> (% ... %) %)".
 * It is output after matching in a tree traversal corresponding to the pattern.
 *
 * Examples:
 *
 * template
 *     (% EnumDecl Name=*
 *         < enum $1.Name { >
 *             ( 
 *                 (% EnumConstantDecl Name=* Type=*
 *                     (% IntegerLiteral Value=*
 *                         < {first?"":","; first = false;} $2.Name = $3.Value >
 *                     %)
 *                 %) |
 *                 (% EnumConstantDecl Name=* Type=*
 *                     < {first?"":","; first = false;} $5 >
 *                 %)
 *              )*
 *         < } >
 *     %)
 *     ;
 */
template: TEMPLATE rexp SEMI ;
rexp : simple_rexp (OR simple_rexp)* ;
simple_rexp : basic_rexp+ ;
basic_rexp : star_rexp | plus_rexp | elementary_rexp ;
star_rexp: elementary_rexp STAR;
plus_rexp: elementary_rexp PLUS;
elementary_rexp: group_rexp | basic ;
group_rexp:   OPEN_PAREN rexp CLOSE_PAREN ;
basic: OPEN_RE ID more* CLOSE_RE ;
more : rexp | text | code | attr ;
text: LANG OTHER_ANG* RANG ;
attr: ID EQ (StringLiteral | STAR);


// CMPT 384 Lecture Notes Robert D. Cameron November 29 - December 1, 1999
// BNF Grammar of Regular Expressions
// Following the precedence rules given previously, a BNF grammar for Perl-style regular expressions can be constructed as follows.
re: simple_re (OR simple_re)*;
simple_re: basic_re+;
basic_re: star | plus | elementary_re;
star: elementary_re STAR;
plus: elementary_re PLUS;
elementary_re: group | any | eos | char | set;
group:   OPEN_PAREN re CLOSE_PAREN;
any: DOT;
eos: DOLLAR;
char:    ID;
set:    positive_set | negative_set;
positive_set:  OPEN_BRACKET set_items CLOSE_BRACKET;
negative_set:  OPEN_BRACKET_NOT set_items CLOSE_BRACKET;
set_items:  set_item+;
set_item:  range | char;
range:    char MINUS char;


#### SpecLexer.g4

lexer grammar SpecLexer;
channels { COMMENTS_CHANNEL }

SINGLE_LINE_DOC_COMMENT: '///' InputCharacter*    -> channel(COMMENTS_CHANNEL);
DELIMITED_DOC_COMMENT:   '/**' .*? '*/'           -> channel(COMMENTS_CHANNEL);
SINGLE_LINE_COMMENT:     '//'  InputCharacter*    -> channel(COMMENTS_CHANNEL);
DELIMITED_COMMENT:       '/*'  .*? '*/'           -> channel(COMMENTS_CHANNEL);

CALLING_CONVENTION :	'calling_convention';
ADD_AFTER_USINGS:	'add_after_usings';
CLASS_NAME	:	'class_name';
CODE		:	'code';
COMPILER_OPTION:	'compiler_option';
DLLIMPORT	:	'dllimport';
EXCLUDE		:	'exclude';
IMPORT_FILE	:	'import_file';
NAMESPACE	:	'namespace';
PREFIX_STRIP	:	'prefix_strip';
TEMPLATE        :	'template';
REWRITE		:	'=>';
EQ		:	'=';
SEMI		:	';';
OR		:	'|';
STAR		:	'*';
PLUS		:	'+';
DOT		:	'.';
DOLLAR		:	'$';
OPEN_RE         :       '(%';
CLOSE_RE        :       '%)';
OPEN_PAREN	:	'(';
CLOSE_PAREN	:	')';
OPEN_BRACKET_NOT:	'[^';
OPEN_BRACKET	:	'[';
CLOSE_BRACKET	:	']';
MINUS		:	'-';
LCURLY		:	'{' -> pushMode(CODE_0);
LANG : '<' -> pushMode(TEXT_0);
StringLiteral	:	'\'' ( Escape | ~('\'' | '\n' | '\r') )* '\'';
ID		:	[a-zA-Z_1234567890.]+ ;

fragment InputCharacter:       ~[\r\n\u0085\u2028\u2029];
fragment Escape : '\'' '\'';
WS:    [ \t\r\n] -> skip;

mode CODE_0;
CODE_0_LCURLY: '{' -> type(OTHER), pushMode(CODE_N);
RCURLY: '}' -> popMode;
CODE_0_OTHER: ~[{}]+ -> type(OTHER);

mode CODE_N;
CODE_N_LCURLY: '{' -> type(OTHER), pushMode(CODE_N);
CODE_N_RCURLY: '}' -> type(OTHER), popMode;
OTHER: ~[{}]+;

mode TEXT_0;
TEXT_0_LANG: '<' -> type(OTHER_ANG), pushMode(TEXT_N);
RANG: '>' -> popMode;
TEXT_0_OTHER: ~[<>]+ -> type(OTHER_ANG);

mode TEXT_N;
TEXT_N_LANG: '<' -> type(OTHER_ANG), pushMode(TEXT_N);
TEXT_N_RANG: '>' -> type(OTHER_ANG), popMode;
OTHER_ANG: ~[<>]+;

(Note: I highly recommend using my [AntlrVSIX](https://marketplace.visualstudio.com/items?itemName=KenDomino.AntlrVSIX) plugin for reading and editing Antlr grammars in Visual Studio 2017!)

## Building Piggy ##

Download [llvm](http://releases.llvm.org/7.0.0/llvm-7.0.0.src.tar.xz),
 [clang](http://releases.llvm.org/7.0.0/cfe-7.0.0.src.tar.xz),
 and [clang extra](http://releases.llvm.org/7.0.0/clang-tools-extra-7.0.0.src.tar.xz).

Untar the downloads. Then,
~~~~
 mv llvm-7.0.0.src llvm               # rename directory to "llvm"
 mv cfe-7.0.0.src llvm/tools/clang;   # move and rename directory to "clang" 
 mv clang-tools-extra-7.0.0.src llvm/tools/clang/tools/extra  # #move and rename
~~~~
 Build using cmake (see [instructions](https://clang.llvm.org/get_started.html)).
~~~~
mkdir build; cd build
cmake -G "Visual Studio 15 2017" -A x64 -Thost=x64 ..\llvm
msbuild LLVM.sln /p:Configuration=Debug /p:Platform=x64
~~~~
Once you have built LLVM and Clang, you can build Piggy.


