using System.IO;

namespace Piggy
{
    internal class InterceptStreamWriter : StreamWriter
    {
        public InterceptStreamWriter(string file_name) : base(file_name) { }

        public override void Write(string value)
        {
            if (value.Contains("public partial struct _"))
            { }
            base.Write(value);
        }

        public override void WriteLine()
        {
            base.WriteLine();
        }

        public override void WriteLine(string value)
        {
            base.WriteLine(value);
        }
    }
}
