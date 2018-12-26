lexer grammar SpecLexer;
channels { COMMENTS_CHANNEL }

SINGLE_LINE_DOC_COMMENT: '///' InputCharacter*    -> channel(COMMENTS_CHANNEL);
DELIMITED_DOC_COMMENT:   '/**' .*? '*/'           -> channel(COMMENTS_CHANNEL);
SINGLE_LINE_COMMENT:     '//'  InputCharacter*    -> channel(COMMENTS_CHANNEL);
DELIMITED_COMMENT:       '/*'  .*? '*/'           -> channel(COMMENTS_CHANNEL);

APPLICATION     :       'application';
CODE            :       'code';
CLANG_FILE      :       'clang_file';
CLANG_OPTION    :       'clang_option';
TEMPLATE        :       'template';
USING           :       'using';
NAMESPACE       :       'namespace';
PASS            :       'pass';
HEADER			:		'header';
INIT			:		'init';
REWRITE         :       '=>';
EQ              :       '=';
COMMA			:		',';
COLON           :       ':';
SEMI            :       ';';
OR              :       '|';
STAR            :       '*';
PLUS            :       '+';
DOT             :       '.';
DOLLAR          :       '$';
OPEN_RE         :       '(%';
CLOSE_RE        :       '%)';
OPEN_PAREN      :       '(';
CLOSE_PAREN     :       ')';
LCURLY          : '{';
RCURLY          : '}';
OPEN_KLEENE_STAR_PAREN  :       '(*';
CLOSE_KLEENE_STAR_PAREN :       '*)';
OPEN_BRACKET_NOT:       '[^';
OPEN_BRACKET    :       '[';
CLOSE_BRACKET   :       ']';
MINUS           :       '-';
LDCURLY          :       '{{' -> pushMode(CODE_0);
LANG : '[[' -> pushMode(TEXT_0);
StringLiteral   :
    ('\'' | '$\'') ( Escape | ~('\'' | '\n' | '\r') )* '\''
    | ('"' | '$"') ( Escape | ~('"' | '\n' | '\r') )* '"';
ID              :       [a-zA-Z_1234567890]+ ;

fragment InputCharacter:       ~[\r\n\u0085\u2028\u2029];
fragment Escape : '\'' '\'';
WS:    [ \t\r\n] -> skip;

mode CODE_0;
CODE_0_LDCURLY: '{{' -> type(OTHER);
RDCURLY: '}}' -> popMode;
OTHER: '}' ~'}' | ~'}' ;

mode TEXT_0;
TEXT_0_LANG: '[[' -> type(OTHER_ANG);
RANG: ']]' -> popMode;
OTHER_ANG: ']' ~']' | ~']' ;
