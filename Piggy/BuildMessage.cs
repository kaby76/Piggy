using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Piggy.Build.Task
{
    internal struct BuildMessage
    {
        private static readonly Regex BuildMessageFormat = new Regex(@"^\s*(?<SEVERITY>[a-z]+)\((?<CODE>[0-9]+)\):\s*((?<FILE>.*):(?<LINE>[0-9]+):(?<COLUMN>[0-9]+):)?\s*(?:syntax error:\s*)?(?<MESSAGE>.*)$", RegexOptions.Compiled);

        public BuildMessage(string message)
            : this(TraceLevel.Error, message, null, 0, 0)
        {
        }

        public BuildMessage(TraceLevel severity, string message, string fileName, int lineNumber, int columnNumber)
            : this()
        {
            Severity = severity;
            Message = message;
            FileName = fileName;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public TraceLevel Severity
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public int LineNumber
        {
            get;
            set;
        }

        public int ColumnNumber
        {
            get;
            set;
        }
    }
}
