grammar Combined;

ast : decl EOF ;
decl : OPEN_PAREN ID more* CLOSE_PAREN ;
more : decl | attr ;
attr : ID EQUALS StringLiteral ;


OPEN_PAREN	:	'(';
CLOSE_PAREN	:	')';
EQUALS		:	'=';
StringLiteral	:	'"' ( Escape | ~('"' | '\n' | '\r') )* '"';
ID		:	[a-zA-Z_1234567890.]+ ;

fragment InputCharacter:       ~[\r\n\u0085\u2028\u2029];
fragment Escape : '\\' '"';
WS:    [ \t\r\n] -> skip;

