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
REWRITE		:	'=>';
SEMI		:	';';
OR		:	'|';
STAR		:	'*';
PLUS		:	'+';
DOT		:	'.';
DOLLAR		:	'$';
OPEN_PAREN	:	'(';
CLOSE_PAREN	:	')';
OPEN_BRACKET_NOT:	'[^';
OPEN_BRACKET	:	'[';
CLOSE_BRACKET	:	']';
MINUS		:	'-';
LCURLY		:	'{' -> pushMode(CODE_0);
StringLiteral	:	'\'' ( Escape | ~('\'' | '\n' | '\r') )* '\'';
ID		:	[a-zA-Z_1234567890.]+ ;

fragment InputCharacter:       ~[\r\n\u0085\u2028\u2029];
fragment Escape : '\'' '\'';
WS:    [ \t\r\n] -> skip;

mode CODE_0;

CODE_0_LCURLY: '{' -> type(OTHER), pushMode(CODE_N);
RCURLY: '}' -> popMode;     // Close for LCURLY
CODE_0_OTHER: ~[{}]+ -> type(OTHER);

mode CODE_N;

CODE_N_LCURLY: '{' -> type(OTHER), pushMode(CODE_N);
CODE_N_RCURLY: '}' -> type(OTHER), popMode;
OTHER: ~[{}]+;

