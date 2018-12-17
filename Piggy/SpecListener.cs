using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;

namespace Piggy
{
    public class SpecListener : SpecParserBaseListener
    {
        Piggy _program;

        public SpecListener(Piggy program)
        {
            _program = program;
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
    }
}
