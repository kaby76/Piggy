using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PiggyRuntime;

namespace PiggyGenerator
{
    public class Piggy
    {
        public Application _application = new Application();
        public IParseTree _ast;
        public List<string> _clang_files = new List<string>();
        public List<string> _clang_options = new List<string>();
        public Dictionary<IParseTree, MethodInfo> _code_blocks = new Dictionary<IParseTree, MethodInfo>();
        public CommonTokenStream _common_token_stream;
        public string _expression;
        public bool _keep_file;
        public string _output_file_name;
        public List<string> _passes = new List<string>();
        public string _specification = string.Empty;
        public string _template_directory;
        public List<Template> _templates = new List<Template>();

        public void RunTool(string ast_file, string spec_file, bool keep_file, string expression,
            string template_directory, string output_file)
        {
            _keep_file = keep_file;
            _expression = expression;
            _specification = spec_file;
            _template_directory = template_directory;
            _output_file_name = output_file;
            Tool.OutputLocation = output_file;
            if (_output_file_name != null)
            {
                var is_file = File.Exists(_output_file_name);
                var is_directory = Directory.Exists(_output_file_name);
                if (is_file)
                    Tool.Redirect = new Redirect(_output_file_name);
            }

            string ast_string = null;
            if (ast_file == null || ast_file == "")
            {
                var s = new List<string>();
                string input;
                while ((input = Console.ReadLine()) != null) s.Add(input);
                ast_string = string.Join("\r\n", s);
            }
            else
            {
                ast_string = File.ReadAllText(ast_file);
            }

            var ast_stream = CharStreams.fromstring(ast_string);
            ITokenSource ast_lexer = new AstLexer(ast_stream);
            var ast_tokens = new CommonTokenStream(ast_lexer);
            var ast_parser = new AstParserParser(ast_tokens);
            ast_parser.BuildParseTree = true;
            var listener = new ErrorListener<IToken>();
            ast_parser.AddErrorListener(listener);
            IParseTree ast_tree = ast_parser.ast();
            if (listener.had_error) throw new Exception();
            _ast = ast_tree;
            _common_token_stream = ast_tokens;
            if (spec_file == null && expression != null)
            {
                var exp = new SpecFileAndListener(this);
                exp.ParseExpressionPattern(expression);
                var output_engine = new OutputEngine(this);
                output_engine.Run(true);
            }
            else if (spec_file != null)
            {
                var full_path =
                    System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(typeof(Piggy).Assembly.Location))
                    + System.IO.Path.DirectorySeparatorChar;
                var file = new SpecFileAndListener(this);
                file.ParseSpecFile(_specification);
                var output_engine = new OutputEngine(this);
                output_engine.Run(false);
            }
        }
    }
}