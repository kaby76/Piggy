namespace PiggyRuntime
{
    using System;
    using System.Collections.Generic;

    public class Template
    {
        public Template() { }

        public string TemplateName { get; set; }

        public List<Pass> Passes { get; set; } = new List<Pass>();

        public string Extends { get; set; }

        public List<Template> ExtendsTemplates { get; set; } = new List<Template>();

        public List<string> Initializations { get; set; } = new List<string>();

        public List<string> Headers { get; set; } = new List<string>();

        public List<Action> InitializationActions { get; set; } = new List<Action>();

        public virtual void ExecuteInitializations()
        {
            foreach (var a in InitializationActions)
                a();
        }

        public Type @Type { get; set; }
    }
}
