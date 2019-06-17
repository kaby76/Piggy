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
using System.Linq;
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
            var include_dirs = tokenFactory.IncludeDirs;
            tokenFactory.pushFilename(filename);
            var ttokens = load(filename, include_dirs);
            List<CPPToken> tokens = ttokens.Select(tt => tt as CPPToken).ToList();
            tokenFactory.popFileName();
            return tokens;
        }

        private static IList<IToken> load(String filename)
        {
            System.Console.Error.WriteLine("opening " + filename);
            try
            {
                // Get current path of file.
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

        public static IList<IToken> load(String filename, List<string> search_path)
        {
            System.Console.Error.WriteLine("opening " + filename);
            var path = Path.GetDirectoryName(filename);
            var is_rooted = System.IO.Path.IsPathRooted(path);
            if (is_rooted)
            {
                return load(filename);
            }

            foreach (var sp in search_path)
            {
                var p = Path.GetFullPath(sp + @"\" + filename);
                try
                {
                    var result = load(p);
                    return result;
                }
                catch (IOException ioe)
                {
                    System.Console.Error.WriteLine("Can't load " + filename);
                }
            }
            return null;
        }
    }
}
