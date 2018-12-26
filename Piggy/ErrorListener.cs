namespace Piggy
{
    using System.IO;
    using Antlr4.Runtime;

    public class ErrorListener<S>  : Antlr4.Runtime.ConsoleErrorListener<S>
    {
        public bool had_error = false;

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, S offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            had_error = true;
            base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
        }
    }
}
