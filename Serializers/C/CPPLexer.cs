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
using Antlr4.Runtime;
using C;
using Engine;

namespace CSerializer
{

    public class CPPLexer : CPPBaseLexer
    {

	protected StackQueue<IToken> buffer = new StackQueue<IToken>();

	public CPPLexer(ICharStream input)
        : base(input)
    { }

    public override IToken NextToken()
    {
		if ( buffer.Count > 0 ) {
			return buffer.DequeueBottom();
		}
		else {
			// matched rule adds at least one to buffer via emit(t)
			base.NextToken(); // ignore return value; we use buffer
			return buffer.DequeueBottom();
		}
	}

    public override IToken Token
    {
        get
        {
            return buffer.PeekBottom(0);
        }
    }

    public override void Emit(IToken token)
    {
		base.Emit(token);
		buffer.Push(token);
	}
}

}