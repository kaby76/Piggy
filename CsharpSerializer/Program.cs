using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace CSharpSerializer
{
    static class Escapes
    {
        public static string provide_escapes(this string s)
        {
            StringBuilder new_s = new StringBuilder();
            for (var i = 0; i != s.Length; ++i)
            {
                if (s[i] == '"' || s[i] == '\\')
                {
                    new_s.Append('\\');
                }
                new_s.Append(s[i]);
            }
            return new_s.ToString();
        }
    }

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
        private static bool first_time = true;

        static void ParenthesizedAST(StringBuilder sb, IParseTree tree, int level = 0)
        {
            if (changed - level >= 0)
            {
                if (!first_time)
                {
                    for (int j = 0; j < level; ++j) sb.Append("  ");
                    for (int k = 0; k < 1 + changed - level; ++k) sb.Append(") ");
                    sb.AppendLine();
                }
                changed = 0;
                first_time = false;
            }
            changed = level;
            for (int j = 0; j < level; ++j) sb.Append("  ");
            // Antlr always names a non-terminal with first letter lowercase,
            // but renames it when creating the type in C#. So, remove the prefix,
            // lowercase the first letter, and remove the trailing "Context" part of
            // the name. Saves big time on output!
            if (tree as TerminalNodeImpl != null)
            {
                sb.AppendLine("( TOKEN t=\"" + tree.GetText().provide_escapes() + "\"");
            }
            else
            {
                var fixed_name = tree.GetType().ToString()
                    .Replace("Antlr4.Runtime.Tree.", "")
                    .Replace("generate_from_spec.CSharpParser+", "")
                    .Replace("CSharpSerializer.CSharpParser+", "")
                    ;
                fixed_name = fixed_name.Substring(0, fixed_name.Length - "Context".Length);
                fixed_name = fixed_name[0].ToString().ToLower()
                             + fixed_name.Substring(1);
                sb.AppendLine("( " + fixed_name);
            }
            for (int i = 0; i < tree.ChildCount; ++i)
            {
                var c = tree.GetChild(i);
                ParenthesizedAST(sb, c, level + 1);
            }
            if (level == 0)
            {
                for (int k = 0; k < 1 + changed - level; ++k) sb.Append(") ");
                sb.AppendLine();
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
            if (listener.had_error) return;
            var sb = new StringBuilder();
            ParenthesizedAST(sb, tree);
            System.Console.WriteLine(sb.ToString());
        }
    }
}
