/***
 * Excerpted from "The Definitive ANTLR 4 Reference",
 * published by The Pragmatic Bookshelf.
 * Copyrights apply to this code. It may not be used to create training material, 
 * courses, books, articles, and the like. Contact us if you are in doubt.
 * We make no guarantees that this code is fit for any purpose. 
 * Visit http://www.pragmaticprogrammer.com/titles/tpantlr2 for more book information.
***/

// don't copy / concat text from tokens, pull char by char in sequence

using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using CSerializer;

namespace CSerializer
{
    public class PreprocessedCharStream : UnbufferedCharStream
    {
        protected IList<IToken> tokens;
        protected List<Interval> tokenCharIntervals;

        protected int tp; // which token are we processing in tokens?
        protected int c = -1; // which char within token text is LA(1)?

        protected String text; // text of current token

        public PreprocessedCharStream(IList<IToken> tokens)
        {
            this.tokens = tokens;
            text = tokens[0].Text;
            computeTokenCharRanges(tokens);
            computeTokenLineRanges(tokens);
        }

        protected override int NextChar()
        {
            if (tp >= tokens.Count)
                return IntStreamConstants.EOF;
            c++;
            if (c == text.Length)
            {
                tp++;
                if (tp == tokens.Count) return IntStreamConstants.EOF;
                c = 0;
                IToken tt = tokens[tp];
                var t = tt as CPPToken;
                text = t.Text;
                base.name = t.filename;
            }

            return text[c];
        }

        public String getFilenameFromCharIndex(int ci)
        {
            int ti = getTokenIndexFromCharIndex(ci);
            if (ti != -1)
            {
                var tt = tokens[ti];
                var t = tt as CPPToken;
                return t.filename;
            }
            return null;
        }

        public int getTokenIndexFromCharIndex(int ci)
        {
            return intervalFor(tokenCharIntervals, ci);
        }

        public int getLineFromCharIndex(int ci)
        {
            int ti = getTokenIndexFromCharIndex(ci);
            if (ti == -1) return -1;
            var tt = tokens[ti];
            CPPToken t = tt as CPPToken;
            int iv = intervalFor(t.lineIntervals, ci); // gives line num from 0
            return iv + t.Line; // add starting line number of entire preprocessed token
        }

        protected void computeTokenCharRanges(IList<IToken> tokens)
        {
            tokenCharIntervals = new ArrayList<Interval>();
            int absCharIndex = 0;
            foreach (var t in tokens)
            {
                int n = t.Text.Length;
                tokenCharIntervals.Add(Interval.Of(absCharIndex, absCharIndex + n - 1));
                absCharIndex += n;
            }

            System.Console.Error.WriteLine(tokenCharIntervals);
        }

        protected void computeTokenLineRanges(IList<IToken> tokens)
        {
            int absCharIndex = 0;
            foreach (var tt in tokens)
            {
                var t = tt as CPPToken;
                t.lineIntervals = new ArrayList<Interval>();
                String text = t.Text;
                int n = text.Length;
                int intervalStart = 0;
                for (int c = 0; c < n; c++)
                {
                    if (text[c] == '\n')
                    {
                        t.lineIntervals.Add(Interval.Of(absCharIndex + intervalStart, absCharIndex + c));
                        intervalStart = c + 1;
                    }
                }

                if (t.lineIntervals.Count == 0)
                {
                    t.lineIntervals.Add(Interval.Of(0, n - 1));
                }

                System.Console.Error.WriteLine("LINE INTERVALS " + t + ":");
                System.Console.Error.WriteLine("\t" + Utils.Join("\n\t",
                      t.lineIntervals
                      ));
                absCharIndex += n;
            }
        }

        protected int intervalFor(List<Interval> intervals, int ci)
        {
            int ti = 0;
            foreach (var iv in intervals)
            {
                if (iv.a <= ci && ci <= iv.b)
                {
                    return ti;
                }

                ti++;
            }

            return -1;
        }
    }
}