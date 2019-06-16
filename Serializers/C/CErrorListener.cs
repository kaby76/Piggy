/***
 * Excerpted from "The Definitive ANTLR 4 Reference",
 * published by The Pragmatic Bookshelf.
 * Copyrights apply to this code. It may not be used to create training material, 
 * courses, books, articles, and the like. Contact us if you are in doubt.
 * We make no guarantees that this code is fit for any purpose. 
 * Visit http://www.pragmaticprogrammer.com/titles/tpantlr2 for more book information.
***/

/** Same same, just adds filename */

using System.IO;
using Antlr4.Runtime;

namespace CSerializer
{

    public class CErrorListener : BaseErrorListener
    {
        public override void SyntaxError(TextWriter output,
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg, RecognitionException e)
        {
            CPPToken token = (CPPToken) offendingSymbol;
            System.Console.Error.WriteLine(token.filename +
                               " line " + line + ":" + charPositionInLine + " at " +
                               token.Text + ": " + msg);
        }
    }
}
