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
	| pass
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
 * () ast match
 * <> text
 * {} code
 *
 * (... (...)) <> vs (... <> (...)). An AST expression matches a set of sub-tree. The
 * first matches the entire sub tree. The later matches the node, then matches additional
 * sub-tree information (presumably for more template processing). In the implementation,
 * the matcher processes in a tree traversal, so templated text is outputed while walking
 * the tree.
 *
 *
 *template ( ParmVarDecl Name=* Type="const wchar_t *"
 *   {
 *        result.Append("int " + tree.Peek(0).Attr("Name") + Environment.NewLine);
 *   }
 *   )
 *   ;
 *
 *template ( ParmVarDecl Name=* Type=*
 *   {
 *        result.Append("int " + tree.Peek(0).Attr("Name") + Environment.NewLine);
 *   }
 *   )
 *   ;
 *
 *template ( FunctionDecl Name=* Type=*
 *   {
 *      result.Append("[DllImport(\"foobar\", CallingConvention = global::System.Runtime.InteropServices.CallingConvention.ThisCall," + Environment.NewLine);
 *      result.Append("\t EntryPoint=\"" + tree.Peek(0).Attr("Name") + "\")]" + Environment.NewLine);
 *      result.Append("internal static extern " + tree.Peek(0).Attr("Type") + " "
 *         + tree.Peek(0).Attr("Name") + "(" + tree.Peek(0).ChildrenOutput() + ");" + Environment.NewLine);
 *   }
 *   )
 *   ;
 *
 *template
 *   ( EnumDecl Name=*
 *      {
 *         vars["first"] = true;
 *         result.Append("enum " + tree.Peek(0).Attr("Name") + "\u007B" + Environment.NewLine);
 *      }
 *      (%
 *         ( EnumConstantDecl Name=*
 *            ( IntegerLiteral Value=*
 *               {
 *                  if ((bool)vars["first"])
 *                     vars["first"] = false;
 *                  else
 *                     result.Append(", ");
 *                  var tt = tree.Peek(1);
 *                  var na = tt.Attr("Name");
 *                  var t2 = tree.Peek(0);
 *                  var va = t2.Attr("Value");
 *                  result.Append(tree.Peek(1).Attr("Name") + " xx= " + tree.Peek(0).Attr("Value") + Environment.NewLine);
 *               }
 *            )
 *         )
 *         |
 *         ( EnumConstantDecl Name=*
 *            {
 *               if ((bool)vars["first"])
 *                  vars["first"] = false;
 *               else
 *                  result.Append(", ");
 *               result.Append(tree.Peek(0).Attr("Name") + Environment.NewLine);
 *            }
 *         )
 *      %)*
 *      {
 *         result.Append("\u007D"); // Closing curly.
 *      }
 *   )
 *   ;
 */
// CMPT 384 Lecture Notes Robert D. Cameron November 29 - December 1, 1999
// BNF Grammar of Regular Expressions
// Following the precedence rules given previously, a BNF grammar for Perl-style regular expressions can be constructed as follows.
template: TEMPLATE rexp SEMI ;
rexp : simple_rexp (OR simple_rexp)* ;
simple_rexp : basic_rexp+ ;
basic_rexp : star_rexp | plus_rexp | elementary_rexp ;
star_rexp: elementary_rexp STAR;
plus_rexp: elementary_rexp PLUS;
elementary_rexp: group_rexp | basic ;
group_rexp:   OPEN_RE rexp CLOSE_RE ;
basic: OPEN_PAREN ID more* CLOSE_PAREN ;
more : rexp | text | code | attr ;
text: LANG OTHER_ANG* RANG ;
attr: ID EQ (StringLiteral | STAR);

/* Specifies the pass for pattern matching. Templates are associated with a
 * pass, matched only for that pass. When the next pass occurs, the pattern matcher
 * is output and reset.
 * It is not required.
 * Example:
 *   pass Enums;
 */
pass: PASS ID SEMI ;
