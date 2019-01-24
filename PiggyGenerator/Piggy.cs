using System.Text;

namespace PiggyGenerator
{
    using System.IO;
    using System;
    using System.Collections.Generic;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using System.Reflection;
    using PiggyRuntime;

    public class Piggy
    {
        public Piggy() { }

        public static string _copyright = @"";
        public List<string> _clang_files = new List<string>();
        public string _specification = string.Empty;
        public string _expression = null;
        public string _template_directory = null;
        public string _output_file_name = null;
        public Redirect _redirect = null;
        public List<string> _clang_options = new List<string>();
        public List<Template> _templates = new List<Template>();
        public Application _application = new Application();
        public IParseTree _ast;
        public List<string> _passes = new List<string>();
        public Dictionary<IParseTree, MethodInfo> _code_blocks = new Dictionary<IParseTree, MethodInfo>();
        public bool _keep_file;

        public void Doit(string ast_file, string spec_file, bool keep_file, string expression, string template_directory, string output_file)
        {
            _keep_file = keep_file;
            _expression = expression;
            _specification = spec_file;
            _template_directory = template_directory;
            _output_file_name = output_file;
            if (output_file != null)
                _redirect = new PiggyRuntime.Redirect(_output_file_name);

            // Parse ast using Antlr.
            // Get back AST as string.
            string ast_string = null;
            if (ast_file == null || ast_file == "")
            {
                List<string> s = new List<string>();
                string input;
                while ((input = Console.ReadLine()) != null)
                {
                    s.Add(input);
                }
                ast_string = string.Join("\r\n", s);
            }
            else
            {
                ast_string = File.ReadAllText(ast_file);
            }
            ICharStream ast_stream = CharStreams.fromstring(ast_string);
            ITokenSource ast_lexer = new AstLexer(ast_stream);
            ITokenStream ast_tokens = new CommonTokenStream(ast_lexer);
            AstParserParser ast_parser = new AstParserParser(ast_tokens);
            ast_parser.BuildParseTree = true;
            ErrorListener<IToken> listener = new ErrorListener<IToken>();
            ast_parser.AddErrorListener(listener);
            IParseTree ast_tree = ast_parser.ast();
            if (listener.had_error) throw new Exception();
            AstSymtabBuilderListener ast_listener = new AstSymtabBuilderListener(ast_tree);
            ParseTreeWalker.Default.Walk(ast_listener, ast_tree);
            _ast = ast_tree;

            if (spec_file == null && expression != null)
            {
                SpecFileAndListener exp = new SpecFileAndListener(this);
                exp.ParseExpressionPattern(expression);
                var output_engine = new OutputEngine(this);
                output_engine.Run(true);
            }
            else if (spec_file != null)
            {
                string full_path = Path.GetDirectoryName(Path.GetFullPath(typeof(Piggy).Assembly.Location))
                                   + Path.DirectorySeparatorChar;
                SpecFileAndListener file = new SpecFileAndListener(this);
                file.ParseSpecFile(_specification);
                var output_engine = new OutputEngine(this);
                output_engine.Run(false);
            }
        }
    }
}
