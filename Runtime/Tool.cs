namespace Runtime
{
    using System.Collections.Generic;
    using System.Linq;

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

        public static string MakeFileNameUnique(string name)
        {
            string path = System.IO.Path.GetDirectoryName(name);
            string file_name = System.IO.Path.GetFileName(name);
            string file_name_wo_suffix = System.IO.Path.GetFileNameWithoutExtension(file_name);
            string ext = System.IO.Path.GetExtension(file_name);
            bool found = GeneratedFiles.Any(s => string.Equals(s, name, System.StringComparison.OrdinalIgnoreCase));
            if (found)
            {
                int counter = 1;
                for (;;)
                {
                    string alt = path + System.IO.Path.DirectorySeparatorChar + file_name_wo_suffix + "-" + counter++ + ext;
                    if (!GeneratedFiles.Any(s => string.Equals(s, alt, System.StringComparison.OrdinalIgnoreCase)))
                        return alt;
                    counter++;
                }
            }
            return name;
        }
    
        public static HashSet<string> GeneratedFiles
        {
            get;
            set;
        } = new HashSet<string>();
    }
}
