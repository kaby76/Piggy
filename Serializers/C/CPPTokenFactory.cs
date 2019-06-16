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
using Antlr4.Runtime.Misc;
using Antlr4.Runtime;

namespace CSerializer
{

    public class CPPTokenFactory : ITokenFactory
    {
        /** Stack of include files */
        Stack<String> stack = new Stack<String>();

        public void pushFilename(String filename)
        {
            stack.Push(filename);
        }

        public void popFileName()
        {
            stack.Pop();
        }

        IToken ITokenFactory.Create(Tuple<ITokenSource, ICharStream> source, int type, string text, int channel, int start, int stop, int line, int charPositionInLine)
        {
            CPPToken t = new CPPToken(source, type, channel, start, stop);
            t.Line = line;
           // t.  setCharPositionInLine(charPositionInLine);
            var input = source.Item2;
            t.Text = input.GetText(Interval.Of(start, stop));
            t.filename = stack.Peek();
            return t;
        }

        public IToken Create(int type, string text)
        {
            return new CPPToken(type, text);
        }
    }
}
