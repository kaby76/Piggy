namespace PiggyRuntime
{
    using System.Collections.Generic;

    public class Pass
    {
        public Pass() { }

        public string Name { get; set; }

        public List<Pattern> Patterns { get; set; } = new List<Pattern>();

        public Template Owner { get; set; }
    }
}
