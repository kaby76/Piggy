grammar AstParser;

options { tokenVocab = AstLexer; }

ast : decl EOF ;

decl : OPEN_PAREN ID attr* child* CLOSE_PAREN ;

attr : ID EQUALS StringLiteral ;

child : decl ;


