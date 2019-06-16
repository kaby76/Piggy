﻿using System;
using gcpp;

namespace CSerializer
{
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using Antlr4.Runtime;
    using Runtime;
    using CommandLine;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Linq;

    class Program
    {
        class Options
        {
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
                    ast_output_file = o.AstOutFile;
                })
                .WithNotParsed(a =>
                {
                    System.Console.WriteLine(a);
                });

            Runtime.Redirect r = null;
            if (ast_output_file != null) r = new Runtime.Redirect(ast_output_file);

            foreach (var filename in arguments)
            {
                CPP.tokenFactory.pushFilename(filename);
                var ts = CPP.load(filename);
                System.Console.WriteLine(ts);
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
                Runtime.AstHelpers.ParenthesizedAST(sb, filename, t);
                System.Console.Error.WriteLine(sb.ToString());

                //var code_as_string = File.ReadAllText(file_name);
                //var input = new AntlrInputStream(code_as_string);
                //var lexer = new CPP14Lexer(input);
                //var tokens = new CommonTokenStream(lexer);
                //var parser = new CPP14Parser(tokens);
                //var listener = new ErrorListener<IToken>();
                //parser.AddErrorListener(listener);
                //var tree = parser.translationunit();
                //if (listener.had_error) return;
                //var sb = new StringBuilder();
                //Runtime.AstHelpers.ParenthesizedAST(sb, file_name, tree);
                //System.Console.Error.WriteLine(sb.ToString());
            }
            if (r != null) r.Dispose();
        }
    }
}
