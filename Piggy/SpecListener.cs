using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;

namespace Piggy
{
    public class SpecListener : SpecParserBaseListener
    {
        Program _program;

        public SpecListener(Program program)
        {
            _program = program;
        }

        public override void ExitNamespace([NotNull] SpecParserParser.NamespaceContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            _program.@namespace = text;
        }

        public override void ExitAdd_after_usings([NotNull] SpecParserParser.Add_after_usingsContext context)
        {
        }

        public override void ExitClass_name([NotNull] SpecParserParser.Class_nameContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            _program.methodClassName = text;
        }

        public override void ExitDllimport([NotNull] SpecParserParser.DllimportContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            text = text.Replace("'", "");
            _program.libraryPath = text;
        }

        public override void ExitCode([NotNull] SpecParserParser.CodeContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            _program.add_after_using = text;
        }

        public override void ExitImport_file([NotNull] SpecParserParser.Import_fileContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            text = text.Replace("'", "");
            _program.files.Add(text);
        }

        public override void ExitExclude([NotNull] SpecParserParser.ExcludeContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            _program.excludeFunctions.Add(text);
        }

        public override void ExitPrefix_strip([NotNull] SpecParserParser.Prefix_stripContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            _program.prefixStrip = text;
        }

        public override void ExitCalling_convention([NotNull] SpecParserParser.Calling_conventionContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            _program.calling_convention = text;
        }

        public override void ExitCompiler_option([NotNull] SpecParserParser.Compiler_optionContext context)
        {
            var c = context.GetChild(1);
            var text = c.GetText();
            text = text.Replace("'", "");
            _program.compiler_options.Add(text);
        }

        public override void ExitTemplate(SpecParserParser.TemplateContext context)
        {
            _program.templates.Add(context);
        }
    }
}
