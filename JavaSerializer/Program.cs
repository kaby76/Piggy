namespace JavaSerializer
{
    using Antlr4.Runtime;
    using PiggyRuntime;
    using CommandLine;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Linq;

    class Program
    {
        class Options
        {
            [Option('f', "java-files", Required = false, HelpText = "C# input files.")]
            public IEnumerable<string> JavaFiles { get; set; }

            [Option('o', "ast-out-file", Required = false, HelpText = "AST output file.")]
            public string AstOutFile { get; set; }
        }

        public class ErrorListener<S> : Antlr4.Runtime.ConsoleErrorListener<S>
        {
            public bool had_error = false;

            public override void SyntaxError(TextWriter output, IRecognizer recognizer, S offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                had_error = true;
                base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
            }
        }

        static void Main(string[] args)
        {
            List<string> options = new List<string>();
            List<string> arguments = new List<string>();
            string ast_output_file = null;

            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    arguments = o.JavaFiles.ToList();
                    ast_output_file = o.AstOutFile;
                })
                .WithNotParsed(a =>
                {
                    System.Console.WriteLine(a);
                });

            PiggyRuntime.Redirect r = new PiggyRuntime.Redirect(ast_output_file);
            foreach (var file_name in arguments)
            {
                var code_as_string = File.ReadAllText(file_name);
                var input = new AntlrInputStream(code_as_string);
                var lexer = new JavaLexer(input);
                var tokens = new CommonTokenStream(lexer);
                var parser = new JavaParser(tokens);
                var listener = new ErrorListener<IToken>();
                parser.AddErrorListener(listener);
                JavaParser.CompilationUnitContext tree = parser.compilationUnit();
                if (listener.had_error) return;
                var sb = new StringBuilder();
                PiggyRuntime.AstHelpers.ParenthesizedAST(sb, file_name, tree);
                System.Console.WriteLine(sb.ToString());
            }
            r.Dispose();
        }
    }
}
