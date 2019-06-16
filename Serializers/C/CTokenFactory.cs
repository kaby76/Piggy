/***
 * Excerpted from "The Definitive ANTLR 4 Reference",
 * published by The Pragmatic Bookshelf.
 * Copyrights apply to this code. It may not be used to create training material, 
 * courses, books, articles, and the like. Contact us if you are in doubt.
 * We make no guarantees that this code is fit for any purpose. 
 * Visit http://www.pragmaticprogrammer.com/titles/tpantlr2 for more book information.
***/

using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using CSerializer;

namespace CSerializer
{

    public class CTokenFactory : ITokenFactory
    {
        private PreprocessedCharStream cinput;

        public CTokenFactory(PreprocessedCharStream cinput)
        {
            this.cinput = cinput;
        }

        public IToken Create(int type, string text)
        {
            return new CToken(type, text);
        }

        public IToken Create(Tuple<ITokenSource, ICharStream> source, int type, string text,
            int channel, int start, int stop, int line, int charPositionInLine)
        {
            CToken t = new CToken(source, type, channel, start, stop);
            t.Line = line;
           // t.setCharPositionInLine(charPositionInLine);
            var input = source.Item2;
            t.Text = input.GetText(Interval.Of(start, stop));
            t.filename = cinput.getFilenameFromCharIndex(start);
            t.Line = cinput.getLineFromCharIndex(start);
            return t;
        }
    }
}