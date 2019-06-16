lexer grammar CPPBaseLexer;

@header {
using CSerializer;
}

INCLUDE
	:	'#include' [ \t]* STRING [ \t]* '\r'? '\n'
		{
			// launch another lexer on the include file, get tokens,
			// emit them all at once here, replacing this token
			var tokens = CPP.Include(Text);
			System.Console.Error.WriteLine("back from include");
				if ( tokens != null )
				{
					foreach (CPPToken t in tokens) Emit(t);
				}
		}
	;

fragment
STRING : '"' .*? '"' ;

OTHER_CMD : '#' ~[\r\n]* '\r'? '\n' ; // can't use .*; scarfs \n\n after include

CHUNK : ~'#'+ ; // anything else
