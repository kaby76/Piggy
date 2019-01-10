namespace PiggyGenerator
{
    using System;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using PiggyRuntime;
    using System.Collections.Generic;

    public class SpecFileAndListener : SpecParserBaseListener
    {
        private Piggy _program;
        private Pass _current_pass;
        private Template _current_template;
        private static int _number_of_applications;
        private HashSet<string> _seen_usings = new HashSet<string>();

        public SpecFileAndListener(Piggy program)
        {
            _program = program;
        }

        public void ParseExpressionPattern(string expression)
        {
            ErrorListener<IToken> listener = new ErrorListener<IToken>();
            ICharStream stream = CharStreams.fromstring(expression);
            ITokenSource lexer = new SpecLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            SpecParserParser parser = new SpecParserParser(tokens);
            parser.BuildParseTree = true;
            parser.AddErrorListener(listener);
            var spec_ast = parser.pattern();
            if (listener.had_error)
            {
                System.Console.WriteLine(spec_ast.GetText());
                throw new Exception();
            }
            var template = new Template();
            _current_template = template;
            template.TemplateName = "Grep";
            _program._templates.Add(template);
            _current_pass = new Pass();
            _current_pass.Name = "Grep";
            _current_pass.Owner = _current_template;
            _current_template.Passes.Add(_current_pass);
            _program._application.OrderedPasses.Add("Grep.Grep");
            ParseTreeWalker.Default.Walk(this, spec_ast);
        }

        public void ParseSpecFile(string specification_file_name)
        {
            ErrorListener<IToken> listener = new ErrorListener<IToken>();
            // Try current location, or template directory.
            string location = specification_file_name;
            if (!System.IO.File.Exists(location))
            {
                location = _program._template_directory + "\\" +
                                     specification_file_name;
                if (!System.IO.File.Exists(location))
                {
                    System.Console.WriteLine("File " + specification_file_name + " does not exist.");
                    throw new Exception();
                }
            }
            ICharStream stream = CharStreams.fromPath(location);
            ITokenSource lexer = new SpecLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            SpecParserParser parser = new SpecParserParser(tokens);
            parser.BuildParseTree = true;
            parser.AddErrorListener(listener);
            var spec_ast = parser.spec();
            if (listener.had_error)
            {
                System.Console.WriteLine(spec_ast.GetText());
                throw new Exception();
            }
            ParseTreeWalker.Default.Walk(this, spec_ast);
        }

        public override void ExitClang_file([NotNull] SpecParserParser.Clang_fileContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            text = text.Replace("'", "");
            _program._clang_files.Add(text);
        }

        public override void ExitClang_option([NotNull] SpecParserParser.Clang_optionContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            text = text.Replace("'", "");
            _program._clang_options.Add(text);
        }

        public override void EnterTemplate(SpecParserParser.TemplateContext context)
        {
            var c = context.GetChild(1);
            var name = c.GetText();
            var template = new Template();
            template.TemplateName = name;
            _program._templates.Add(template);
            _current_template = template;
        }

        public override void EnterExtends(SpecParserParser.ExtendsContext context)
        {
            // Add all items in the extends list.
            var start = 1;
            var end = context.ChildCount;
            for (int i = start; i < end; i += 2)
            {
                var c = context.GetChild(i);
                var id = c.GetText();
                _current_template.Extends = id;
                break;
            }
        }

        public override void EnterPass(SpecParserParser.PassContext context)
        {
            var current_template = _current_template;
            var current_pass = new Pass();
            current_pass.Owner = current_template;
            current_template.Passes.Add(current_pass);
            var c = context.GetChild(1);
            var pass_name = c.GetText();
            current_pass.Name = pass_name;
            _current_pass = current_pass;
        }

        public override void EnterHeader(SpecParserParser.HeaderContext context)
        {
            var current_template = _current_template;
            if (context.GetText() == "") return;
            var c = context.GetChild(1);
            var code = c.GetText();
            code = code.Substring(2);
            code = code.Substring(0, code.Length - 2);
            current_template.Headers.Add(code);
        }

        public override void EnterInit(SpecParserParser.InitContext context)
        {
            var current_template = _current_template;
            if (context.GetText() == "") return;
            var c = context.GetChild(1);
            var code = c.GetText();
            code = code.Substring(2);
            code = code.Substring(0, code.Length - 2);
            current_template.Initializations.Add(code);
        }

        public override void EnterPattern(SpecParserParser.PatternContext context)
        {
            var current_pass = _current_pass;
            var pattern = new Pattern();
            pattern.Owner = current_pass;
            current_pass.Patterns.Add(pattern);
            pattern.AstNode = context;
        }

        public override void EnterUsing(SpecParserParser.UsingContext context)
        {
            // When including another spec file, build a tree from the spec file
            // and insert it here.
            var c = context.GetChild(1);
            var text = c.GetText();
            text = text.Replace("'", "");
            if (_seen_usings.Contains(text)) return;
            _seen_usings.Add(text);
            SpecFileAndListener file = new SpecFileAndListener(_program);
            file.ParseSpecFile(text);
        }

        public override void EnterApplication(SpecParserParser.ApplicationContext context)
        {
            if (_number_of_applications++ > 0)
                throw new Exception("More than one APPLICATION--an application is a sequence of pattern matching passes. I assume one would suffice, two you didn't mean it.");
        }

        public override void EnterApply_pass(SpecParserParser.Apply_passContext context)
        {
            // Record the order of how the user wants to apply template passes.
            var text = context.GetText();
            _program._application.OrderedPasses.Add(text);
        }
    }
}
