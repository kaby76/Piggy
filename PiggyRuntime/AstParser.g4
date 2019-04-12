grammar AstParser;

options { tokenVocab = AstLexer; }

ast : node EOF ;
node : OPEN_PAREN ID (node | attr)* CLOSE_PAREN ;
attr : ID EQUALS StringLiteral ;
 