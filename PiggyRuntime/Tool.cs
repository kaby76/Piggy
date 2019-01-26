namespace PiggyRuntime
{
    using System.Collections.Generic;

    public class Tool
    {
        // This class is used by the tool and the templates for communication between the two.
        // It indicates the options used for the tool invocation.

        private static string _output_location;
        public static string OutputLocation
        {
            get { return _output_location; }
            set
            {
                if (System.IO.Directory.Exists(value) && !(value.EndsWith("\\") || value.EndsWith("/")))
                    _output_location = value + "\\";
                else
                    _output_location = value;
            }
        }

        public static string[] CommandLineArgs
        {
            get;
            set;
        }

        public static Redirect Redirect
        {
            get;
            set;
        }

        public static List<string> GeneratedFiles
        {
            get;
            set;
        } = new List<string>();
    }
}
