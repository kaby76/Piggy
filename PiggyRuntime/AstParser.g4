grammar AstParser;

options { tokenVocab = AstLexer; }

ast : decl EOF ;
decl : OPEN_PAREN ID more* CLOSE_PAREN ;
more : decl | attr ;
attr : ID EQUALS StringLiteral ;
 