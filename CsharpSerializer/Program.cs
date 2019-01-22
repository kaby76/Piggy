using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace CSharpSerializer
{
    class Program
    {
        public class ErrorListener<S> : Antlr4.Runtime.ConsoleErrorListener<S>
        {
            public bool had_error = false;

            public override void SyntaxError(TextWriter output, IRecognizer recognizer, S offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                had_error = true;
                base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
            }
        }

        private static int changed = 0;

        static void ParenthesizedAST(StringBuilder sb, IParseTree tree, int level = 0)
        {
            changed = level;
            for (int j = 0; j < level; ++j) sb.Append("  ");
            var fixed_name = tree.GetType().ToString()
                .Replace("Antlr4.Runtime.Tree.", "")
                .Replace("generate_from_spec.CSharpParser+", "")
                .Replace("CSharpSerializer.CSharpParser+", "");
            sb.Append("( " + fixed_name);
            if (tree as TerminalNodeImpl != null) sb.AppendLine(" " + tree.GetText());
            else sb.AppendLine();
            for (int i = 0; i < tree.ChildCount; ++i)
            {
                var c = tree.GetChild(i);
                ParenthesizedAST(sb, c, level + 1);
                if (changed > 0)
                {
                    for (int k = 0; k < level + 1; ++k)
                        sb.Append("  ");
                    for (int k = 0; k < changed - level; ++k)
                        sb.Append(") ");
                    sb.AppendLine();
                    changed = 0;
                }
            }
            if (level == 0)
            {
                for (int k = 0; k < changed - level; ++k)
                    sb.Append(") ");
                sb.AppendLine(")");
                changed = 0;
            }
        }

        static void Reconstruct(IParseTree tree, CommonTokenStream stream)
        {
            if (tree as TerminalNodeImpl != null)
            {
                TerminalNodeImpl tok = tree as TerminalNodeImpl;
                Interval interval = tok.SourceInterval;
                var inter = stream.GetHiddenTokensToLeft(tok.Symbol.TokenIndex);
                if (inter != null)
                    foreach (var t in inter)
                    {
                        System.Console.Write(t.Text);
                    }
                var s = stream.GetText(interval);
                System.Console.Write(s);
            }
            else
            {
                for (int i = 0; i < tree.ChildCount; ++i)
                {
                    var c = tree.GetChild(i);
                    Reconstruct(c, stream);
                }
            }
        }

        static void Main(string[] args)
        {
            var file_name = @"c:\Users\Kenne\source\repos\generate-from-spec\spec-20.cs";
            var code_as_string = File.ReadAllText(file_name);
            var input = new AntlrInputStream(code_as_string);
            var lexer = new CSharpLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new CSharpParser(tokens);
            var listener = new ErrorListener<IToken>();
            parser.AddErrorListener(listener);
            CSharpParser.Compilation_unitContext tree = parser.compilation_unit();
            System.Console.WriteLine(listener.had_error ? "Didn't work" : "Worked");
            // Parenthesized tree expression output.
            var sb = new StringBuilder();
            ParenthesizedAST(sb, tree);
            System.Console.WriteLine(sb.ToString());
            //Reconstruct(tree, tokens);
        }

    }
}
