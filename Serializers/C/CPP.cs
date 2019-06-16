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
        static CPPTokenFactory tokenFactory = new CPPTokenFactory();

        public static List<CPPToken> Include(String includeCommand)
        {
            System.Console.WriteLine("process " + includeCommand);
            int l = includeCommand.IndexOf('"');
            int r = includeCommand.LastIndexOf('"');
            String filename = includeCommand.Substring(l + 1, r);
            tokenFactory.pushFilename(filename);
            List<CPPToken> tokens = load(filename);
            tokenFactory.popFileName();
            return tokens;
        }

        static List<CPPToken> load(String filename)
        {
            System.Console.WriteLine("opening " + filename);
            try
            {
                var code_as_string = File.ReadAllText(filename);
                var input = new AntlrInputStream(code_as_string);
                CPPLexer lexer = new CPPLexer(input);
                lexer.TokenFactory = tokenFactory;
                return (List<CPPToken>)lexer.GetAllTokens();
            }
            catch (IOException ioe)
            {
                System.Console.Error.WriteLine("Can't load " + filename);
            }
            return null;
        }

        public static void main(String[] args)
        {
            String filename = args[0];
            tokenFactory.pushFilename(filename);
            List<CPPToken> tokens = load(filename);
            System.Console.WriteLine(tokens);

            PreprocessedCharStream cinput = new PreprocessedCharStream(tokens);
            var clexer = new gcpp.CPP14Lexer(cinput);
            // force creation of CPPTokensm set file,line
            clexer.TokenFactory = new CTokenFactory(cinput);
            CommonTokenStream ctokens = new CommonTokenStream(clexer);
            var cparser = new gcpp.CPP14Parser(ctokens);
            cparser.RemoveErrorListeners();
            cparser.AddErrorListener(new CErrorListener());
            var t = cparser.translationunit();
            var sb = new StringBuilder();
            Runtime.AstHelpers.ParenthesizedAST(sb, filename, t);
            System.Console.Error.WriteLine(sb.ToString());
        }

    }
}
