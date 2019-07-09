using System;
using Antlr4.Runtime;
using CommandLine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Antlr4.Runtime.Tree.Pattern;
using C;
using Runtime;

namespace CSerializer
{
    class Program
    {
        class Options
        {
            [Option('c', "compiler-option", Required = false, HelpText = "Compiler option.")]
            public IEnumerable<string> CompilerOptions { get; set; }

            [Option('f', "c-files", Required = true, HelpText = "C input files.")]
            public IEnumerable<string> CFiles { get; set; }

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
                    arguments = o.CFiles.ToList();
                    options = o.CompilerOptions.ToList();
                    ast_output_file = o.AstOutFile;
                })
                .WithNotParsed(a =>
                {
                    System.Console.Error.WriteLine(a);
                });

            Runtime.Redirect r = null;
            if (ast_output_file != null) r = new Runtime.Redirect(ast_output_file);

            foreach (var filename in arguments)
            {
                var include_dirs = CPP.tokenFactory.IncludeDirs;
                CPP.tokenFactory.pushFilename(filename);
                var ts = CPP.load(filename, include_dirs);
                System.Console.Error.WriteLine(String.Join("\n",ts.Select(x => x.ToString())));
                PreprocessedCharStream cinput = new PreprocessedCharStream(ts);
                var clexer = new gcpp.CPP14Lexer(cinput);
                // force creation of CPPTokensm set file,line
                clexer.TokenFactory = new CTokenFactory(cinput);
                CommonTokenStream ctokens = new CommonTokenStream(clexer);
                var cparser = new gcpp.CPP14Parser(ctokens);
                cparser.RemoveErrorListeners();
                cparser.AddErrorListener(new CErrorListener());
                var t = cparser.translationunit();
                var sb = new StringBuilder();
                var ser = new Runtime.AstHelpers();
                ser.ParenthesizedAST(sb, filename, t, ctokens);
                System.Console.WriteLine(sb.ToString());
            }
            if (r != null) r.Dispose();
        }
    }
}
