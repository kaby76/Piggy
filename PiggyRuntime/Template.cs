namespace PiggyRuntime
{
    using org.antlr.symtab;
    using System;
    using System.Collections.Generic;

    public class Template
    {
        public Template()
        {
            _stack.Push(_symbol_table.GLOBALS);
        }

        public string TemplateName { get; set; }

        public List<Pass> Passes { get; set; } = new List<Pass>();

        public string Extends { get; set; }

        public List<Template> ExtendsTemplates { get; set; } = new List<Template>();

        public List<string> Initializations { get; set; } = new List<string>();

        public List<string> Headers { get; set; } = new List<string>();

        public List<Action> InitializationActions { get; set; } = new List<Action>();

        public System.Type @Type { get; set; }

        protected static SymbolTable _symbol_table = new SymbolTable();

        protected static Stack<Scope> _stack = new Stack<Scope>();
    }
}
