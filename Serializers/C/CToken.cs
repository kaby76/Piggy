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

namespace CSerializer
{

    public class CToken : CommonToken
    {
        public String filename;

        public CToken()
            : base(0, "")
        {
        }

        public CToken(int type, String text)
            : base(type, text)
        {
        }

        public CToken(System.Tuple<Antlr4.Runtime.ITokenSource, Antlr4.Runtime.ICharStream> source,
            int type, int channel, int start, int stop)
            : base(source, type, channel, start, stop)
        {
        }


        public override string ToString()
        {
            String t = base.ToString();
            return filename + ":" + t;
        }
    }
}
