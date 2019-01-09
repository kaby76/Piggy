using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Piggy.Build.Task
{
    public class PiggyClassGenerationTask : Microsoft.Build.Utilities.Task
    {
        private List<ITaskItem> _generatedCodeFiles = new List<ITaskItem>();

        [Required]
        public string ToolPath
        {
            get;
            set;
        }

        [Required]
        public string OutputPath
        {
            get;
            set;
        }

        [Required]
        public string SourceCodeFiles
        {
            get;
            set;
        }

        public string ClangOptions
        {
            get;
            set;
        }

        [Output]
        public ITaskItem[] GeneratedCodeFiles
        {
            get
            {
                return this._generatedCodeFiles.ToArray();
            }
            set
            {
                this._generatedCodeFiles = new List<ITaskItem>(value);
            }
        }

        private static string JoinArguments(IEnumerable<string> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            StringBuilder builder = new StringBuilder();
            foreach (string argument in arguments)
            {
                if (builder.Length > 0)
                    builder.Append(' ');

                if (argument.IndexOfAny(new[] { '"', ' ' }) < 0)
                {
                    builder.Append(argument);
                    continue;
                }

                // escape a backslash appearing before a quote
                string arg = argument.Replace("\\\"", "\\\\\"");
                // escape double quotes
                arg = arg.Replace("\"", "\\\"");

                // wrap the argument in outer quotes
                builder.Append('"').Append(arg).Append('"');
            }

            return builder.ToString();
        }

        public override bool Execute()
        {
            bool success = false;
            try
            {
                List<string> arguments = new List<string>();
                arguments.Add("-o");
                arguments.Add(OutputPath);
                arguments.Add("-c");
                arguments.Add(ClangOptions);
                arguments.Add("-f");
                arguments.Add(SourceCodeFiles);

                using (Process process = new Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = "ClangSerializer.exe";
                    process.StartInfo.Arguments = JoinArguments(arguments);
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    process.StandardInput.Dispose();
                    process.WaitForExit();
                    if (process.ExitCode != 0) success = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                success = false;
            }
            return success;
        }
    }
}
