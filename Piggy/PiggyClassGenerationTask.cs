using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Piggy.Build.Task
{
    public class PiggyClassGenerationTask : Microsoft.Build.Utilities.Task
    {
        private List<ITaskItem> _generatedCodeFiles = new List<ITaskItem>();

        [Required]
        public string OutputPath
        {
            get;
            set;
        }

        public string ClangOptions
        {
            get;
            set;
        }

        public string ClangSourceFile
        {
            get;
            set;
        }

        public string AstOutputFile
        {
            get;
            set;
        }

        public string InitialTemplate
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

                continue;

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
            bool success = true;
            try
            {
                List<string> arguments = new List<string>();

                string path = Assembly.GetAssembly(typeof(PiggyClassGenerationTask)).Location;
                path = Path.GetDirectoryName(path);
                path = Path.GetFullPath(path + @"\..\..\");
                path = path + @"\build\ClangSerializer.dll";
                arguments.Add("\"" + path + "\"");

                if (ClangOptions != null)
                {
                    arguments.Add("-c");
                    var str = ClangOptions;
                    if (!Regex.IsMatch(str, @"^"".*""$"))
                    {
                        str = "\"" + str + "\"";
                    }
                    arguments.Add(str);
                }
                if (ClangSourceFile != null)
                {
                    arguments.Add("-f");
                    var str = ClangSourceFile;
                    if (!Regex.IsMatch(str, @"^"".*""$"))
                    {
                        str = "\"" + str + "\"";
                    }
                    arguments.Add(str);
                }
                if (AstOutputFile != null)
                {
                    arguments.Add("-o");
                    var str1 = OutputPath;
                    var str2 = AstOutputFile;
                    string p = "\"" + str1 + "\\" + str2 + "\"";
                    arguments.Add(p);
                }

                using (Process process = new Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = "dotnet.exe";
                    var a = String.Join(" ", arguments);
                    process.StartInfo.Arguments = a;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit();
                    if (process.ExitCode != 0) success = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                success = false;
            }

            if (!success) return success;

            try
            {
                List<string> arguments = new List<string>();

                string path = Assembly.GetAssembly(typeof(PiggyClassGenerationTask)).Location;
                path = Path.GetDirectoryName(path);
                path = Path.GetFullPath(path + @"\..\..\");
                path = path + @"\build\PiggyTool.dll";
                arguments.Add("\"" + path + "\"");

                if (AstOutputFile != null)
                {
                    arguments.Add("-a");
                    string p = OutputPath + "\\" + AstOutputFile;
                    arguments.Add(p);
                }
                if (InitialTemplate != null)
                {
                    arguments.Add("-s");
                    arguments.Add(InitialTemplate);
                }

                {
                    string tpath = Assembly.GetAssembly(typeof(PiggyClassGenerationTask)).Location;
                    tpath = Path.GetDirectoryName(tpath);
                    tpath = Path.GetFullPath(tpath + @"\..\..\");
                    tpath = tpath + @"\Templates";
                    arguments.Add("-t");
                    arguments.Add("\"" + tpath + "\"");
                }

                {
                    arguments.Add("-o");
                    var p = "\"" + OutputPath + "\\" + Path.GetFileName(InitialTemplate) + ".cs" + "\"";
                    arguments.Add(p);
                }

                using (Process process = new Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = "dotnet.exe";
                    var a = String.Join(" ", arguments);
                    process.StartInfo.Arguments = a;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit();
                    if (process.ExitCode != 0) success = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                success = false;
            }

            if (!success) return success;

            _generatedCodeFiles.Add((ITaskItem)new TaskItem(OutputPath + "\\" + Path.GetFileName(InitialTemplate) + ".cs"));

            return success;
        }
    }
}
