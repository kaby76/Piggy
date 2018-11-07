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
