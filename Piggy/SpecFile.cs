using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Piggy
{
    public class SpecFile : SpecParserBaseListener
    {
        Piggy _program;

        public SpecFile(Piggy program)
        {
            _program = program;
        }

        public void ParseSpecFile(string specification)
        {
            ErrorListener<IToken> listener = new ErrorListener<IToken>();
            if (!System.IO.File.Exists(specification))
            {
                System.Console.WriteLine("File " + specification + " does not exist.");
                throw new Exception();
            }
            ICharStream stream = CharStreams.fromPath(specification);
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

        public override void ExitCode([NotNull] SpecParserParser.CodeContext context)
        {
            _program._code_blocks[context] = null;
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

        public override void ExitTemplate(SpecParserParser.TemplateContext context)
        {
            _program._templates[_program._templates.Count - 1].Add(context);
        }

        public override void EnterPass(SpecParserParser.PassContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            _program._passes.Add(text);
            _program._templates.Add(new List<SpecParserParser.TemplateContext>());
        }

        public override void ExitExtends(SpecParserParser.ExtendsContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            _program._extends = text;
        }

        public override void ExitNamespace(SpecParserParser.NamespaceContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            _program._namespace = text;
        }

        public override void ExitHeader(SpecParserParser.HeaderContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            text = text.Substring(2);
            text = text.Substring(0, text.Length - 2);
            _program._header = text;
        }

        public override void EnterInclude(SpecParserParser.IncludeContext context)
        {
            // When including another spec file, build a tree from the spec file
            // and insert it here.
            var c = context.GetChild(1);
            var text = c.GetText();
            text = text.Replace("'", "");
            SpecFile file = new SpecFile(_program);
            file.ParseSpecFile(text);
        }
    }
}
