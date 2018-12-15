grammar SpecParser;

options { tokenVocab = SpecLexer; }

spec : items* EOF ;

items
    : using
    | import_file
    | compiler_option
    | template
    | pass
    ;

/* Specifies the full path of an assembly to link into for support in code generation.
 * This will be accessible to the code blocks you use in templates.
 */
using: USING StringLiteral SEMI ;

/* Specifies an input file for the Clang compiler. Use forward slashes for directory
 * delimiters.
 * Example:
 *   import_file 'c:/Program Files/NVIDIA GPU Computing Toolkit/cuda/v10.0/include/cuda.h';
 */
import_file: IMPORT_FILE StringLiteral SEMI ;

/* Specifies an additional Clang compiler option. Use forward slashes for directory
 * delimiters. Use multiple times to specify more than one option.
 * Example:
 *   compiler_options '--target=x86_64';
 *   compiler_options '-Ic:/Program Files/NVIDIA GPU Computing Toolkit/cuda/v10.0/include';
 */
compiler_option: COMPILER_OPTION StringLiteral SEMI ;

/* Specifies a pattern matching expression for output.
 *
 * (... (...)) <> vs (... <> (...)). An AST expression matches a set of sub-tree. The
 * first matches the entire sub tree. The later matches the node, then matches additional
 * sub-tree information (presumably for more template processing). In the implementation,
 * the matcher processes in a tree traversal, so templated text is outputed while walking
 * the tree.
 */

// Note: the regular expression grammar is based on that of Cameron.
template: TEMPLATE rexp SEMI ;
rexp : simple_rexp (OR simple_rexp)* ;
simple_rexp : basic_rexp+ ;
basic_rexp : star_rexp | plus_rexp | elementary_rexp ;
star_rexp: elementary_rexp STAR;
plus_rexp: elementary_rexp PLUS;
elementary_rexp: group_rexp | basic ;
group_rexp:   OPEN_RE rexp CLOSE_RE ;
basic: simple_basic | kleene_star_basic ;
simple_basic: OPEN_PAREN id_or_star_or_empty more* CLOSE_PAREN ;
kleene_star_basic: OPEN_KLEENE_STAR_PAREN id_or_star_or_empty more* CLOSE_KLEENE_STAR_PAREN ;
id_or_star_or_empty: ID | STAR | /* epsilon */ ;
more : rexp | text | code | attr ;
code: LCURLY OTHER* RCURLY ;
text: LANG OTHER_ANG* RANG ;
attr: ID EQ (StringLiteral | STAR);
/*
// CMPT 384 Lecture Notes Robert D. Cameron November 29 - December 1, 1999
// BNF Grammar of Regular Expressions
// http://www.cs.sfu.ca/~cameron/Teaching/384/99-3/regexp-plg.html
<RE>	::=	<union> | <simple-RE>
<union>	::=	<RE> "|" <simple-RE>
<simple-RE>	::=	<concatenation> | <basic-RE>
<concatenation>	::=	<simple-RE> <basic-RE>
<basic-RE>	::=	<star> | <plus> | <elementary-RE>
<star>	::=	<elementary-RE> "*"
<plus>	::=	<elementary-RE> "+"
<elementary-RE>	::=	<group> | <any> | <eos> | <char> | <set>
<group>	::=	"(" <RE> ")"
<any>	::=	"."
<eos>	::=	"$"
<char>	::=	any non metacharacter | "\" metacharacter
<set>	::=	<positive-set> | <negative-set>
<positive-set>	::=	"[" <set-items> "]"
<negative-set>	::=	"[^" <set-items> "]"
<set-items>	::=	<set-item> | <set-item> <set-items>
<set-items>	::=	<range> | <char>
<range>	::=	<char> "-" <char>
 */

/* Specifies the pass for pattern matching. Templates are associated with a
 * pass, matched only for that pass. When the next pass occurs, the pattern matcher
 * is output and reset.
 * It is not required.
 * Example:
 *   pass Enums;
 */
pass: PASS ID SEMI ;
