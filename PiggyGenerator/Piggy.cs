using CommandLine;
using Microsoft.CodeAnalysis;

namespace PiggyGenerator
{
    using System.IO;
    using System.Runtime.InteropServices;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using System.Text;
    using System.Reflection;
    using org.antlr.symtab;
    using PiggyRuntime;

    public class Piggy
    {
        public Piggy() { }

        public static string _copyright = @"";
        public List<string> _clang_files = new List<string>();
        public string _specification = string.Empty;
        public List<string> _clang_options = new List<string>();
        public bool _display_ast = false;
        public Dictionary<string, List<SpecParserParser.TemplateContext>> _patterns = new Dictionary<string, List<SpecParserParser.TemplateContext>>();
        public List<Template> _templates = new List<Template>();
        public Application _application = new Application();
        public IParseTree _ast;
        public List<string> _passes = new List<string>();
        public Dictionary<IParseTree, MethodInfo> _code_blocks = new Dictionary<IParseTree, MethodInfo>();
        public SymbolTable _symbol_table;
        public string _extends = "";
        public List<string> _header = new List<string>();
        public IParseTree _header_context = null;
        public List<string> _referenced_assemblies = new List<string>();

        class Options
        {
            [Option('a', "clang-ast-file", Required = false, HelpText = "Clang ast input file.")]
            public string ClangFile { get; set; }

            [Option('s', "piggy-spec-file", Required = true, HelpText = "Piggy spec input file.")]
            public string PiggyFile { get; set; }
        }

        public void Doit(string[] args)
        {
            string temp_fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cpp";
            ErrorListener<IToken> listener = new ErrorListener<IToken>();
            try
            {
                string full_path = Path.GetDirectoryName(Path.GetFullPath(typeof(Piggy).Assembly.Location))
                                   + Path.DirectorySeparatorChar;
                string ast_file = null;
                string spec_file = null;
                CommandLine.Parser.Default.ParseArguments<Options>(args)
                    .WithParsed<Options>(o =>
                    {
                        ast_file = o.ClangFile;
                        spec_file = o.PiggyFile;
                    })
                    .WithNotParsed(a =>
                    {
                        System.Console.WriteLine(a);
                    });

                _specification = spec_file;

                var errorList = new List<string>();

                SpecFileAndListener file = new SpecFileAndListener(this);
                file.ParseSpecFile(_specification);

                // Get back AST as string.
                string ast_string = null;
                if (ast_file == null || ast_file == "")
                {
                    List<string> s = new List<string>();
                    string input;
                    while ((input = Console.ReadLine()) != null && input != "")
                    {
                        s.Add(input);
                    }
                    ast_string = string.Join(" ", s);
                }
                else
                {
                    ast_string = File.ReadAllText(ast_file);
                }

                // Parse ast using Antlr.
                ICharStream ast_stream = CharStreams.fromstring(ast_string);
                ITokenSource ast_lexer = new AstLexer(ast_stream);
                ITokenStream ast_tokens = new CommonTokenStream(ast_lexer);
                AstParserParser ast_parser = new AstParserParser(ast_tokens);
                ast_parser.BuildParseTree = true;
                ast_parser.AddErrorListener(listener);
                IParseTree ast_tree = ast_parser.ast();
                if (listener.had_error) throw new Exception();
                AstSymtabBuilderListener ast_listener = new AstSymtabBuilderListener(ast_tree);
                ParseTreeWalker.Default.Walk(ast_listener, ast_tree);
                _ast = ast_tree;

                //System.Console.WriteLine("AST parsed");
                // Find and apply ordered regular expression templates until done.
                // Templates contain code, which has to be compiled and run.
                var output_engine = new OutputEngine(this);
                System.Console.WriteLine(output_engine.Run());
            }
            finally
            {
                File.Delete(temp_fileName);
            }
        }

        void SetUpSymbolTable()
        {
            // Create symbol table for AST.
            _symbol_table = new SymbolTable();
            AstSymtabBuilderListener listener = new AstSymtabBuilderListener(_ast);
            ParseTreeWalker.Default.Walk(listener, _ast);
        }

    }
}
