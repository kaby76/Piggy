/***
 * Excerpted from "The Definitive ANTLR 4 Reference",
 * published by The Pragmatic Bookshelf.
 * Copyrights apply to this code. It may not be used to create training material, 
 * courses, books, articles, and the like. Contact us if you are in doubt.
 * We make no guarantees that this code is fit for any purpose. 
 * Visit http://www.pragmaticprogrammer.com/titles/tpantlr2 for more book information.
***/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;

namespace CSerializer
{
    public class CPP
    {
        public static CPPTokenFactory tokenFactory = new CPPTokenFactory();

        public static List<CPPToken> Include(String includeCommand)
        {
            System.Console.Error.WriteLine("process " + includeCommand);
            int l = includeCommand.IndexOf('"');
            int r = includeCommand.LastIndexOf('"');
            String filename = includeCommand.Substring(l + 1, r - (l+1));
            tokenFactory.pushFilename(filename);
            var ttokens = load(filename);
            List<CPPToken> tokens = ttokens as List<CPPToken>;
            tokenFactory.popFileName();
            return tokens;
        }

        public static IList<IToken> load(String filename)
        {
            System.Console.Error.WriteLine("opening " + filename);
            try
            {
                var code_as_string = File.ReadAllText(filename);
                var input = new AntlrInputStream(code_as_string);
                CPPLexer lexer = new CPPLexer(input);
                lexer.TokenFactory = tokenFactory;
                return lexer.GetAllTokens();
            }
            catch (IOException ioe)
            {
                System.Console.Error.WriteLine("Can't load " + filename);
            }
            return null;
        }
    }
}
