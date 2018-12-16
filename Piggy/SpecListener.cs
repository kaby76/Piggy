﻿using System;
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

        public override void ExitImport_file([NotNull] SpecParserParser.Import_fileContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            text = text.Replace("'", "");
            _program._files.Add(text);
        }

        public override void ExitCompiler_option([NotNull] SpecParserParser.Compiler_optionContext context)
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

        public override void ExitPass(SpecParserParser.PassContext context)
        {
        }

        public override void EnterPass(SpecParserParser.PassContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            _program._passes.Add(text);
            _program._templates.Add(new List<SpecParserParser.TemplateContext>());
        }

        public override void ExitUsing(SpecParserParser.UsingContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            _program._usings.Add(text);
        }
    }
}
