// Piggy parser grammar--turns DFS tree visitors for conversion inside out!
grammar SpecParser;

options { tokenVocab = SpecLexer; }

spec : clang* using* template* (application|) EOF ;

application: APPLICATION apply_pass* SEMI ;
apply_pass: ID DOT ID ;

clang: clang_file | clang_option;

/* Specifies an input file for the Clang compiler. Use forward slashes for directory
 * delimiters.
 * Example:
 *   import_file 'c:/Program Files/NVIDIA GPU Computing Toolkit/cuda/v10.0/include/cuda.h';
 */
clang_file: CLANG_FILE StringLiteral SEMI ;

/* Specifies an additional Clang compiler option. Use forward slashes for directory
 * delimiters. Use multiple times to specify more than one option.
 * Example:
 *   compiler_options '--target=x86_64';
 *   compiler_options '-Ic:/Program Files/NVIDIA GPU Computing Toolkit/cuda/v10.0/include';
 */
clang_option: CLANG_OPTION StringLiteral SEMI ;

using: USING StringLiteral SEMI ;

template: TEMPLATE ID extends LCURLY header init pass* RCURLY ;

extends: COLON ID | ;

header: HEADER code | ;

init: INIT code | ;

/* Specifies the pass for pattern matching. Templates are associated with a
 * pass, matched only for that pass. When the next pass occurs, the pattern matcher
 * is output and reset.
 * It is not required.
 * Example:
 *   pass Enums;
 */
pass: PASS ID LCURLY pattern* RCURLY ;

// Note: the regular expression grammar is based on that of Cameron.
pattern: basic ;
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
code: LDCURLY OTHER* RDCURLY ;
text: LANG OTHER_ANG* RANG ;
attr: ID EQ (StringLiteral | STAR) | NOT ID;
/*
// CMPT 384 Lecture Notes Robert D. Cameron November 29 - December 1, 1999
// BNF Grammar of Regular Expressions
// http://www.cs.sfu.ca/~cameron/Teaching/384/99-3/regexp-plg.html
*/
