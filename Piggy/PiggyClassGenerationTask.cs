﻿using System;
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
        private List<BuildMessage> _buildMessages = new List<BuildMessage>();

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
                    if (Regex.IsMatch(str, @"^"".*""$"))
                    {
                        // strip "".
                        str = str.Substring(1);
                        str = str.Substring(str.Length - 1);
                    }
                    str = "\"" + str + "\"";
                    arguments.Add(str);
                }
                if (AstOutputFile != null)
                {
                    arguments.Add("-o");
                    var ostr = OutputPath;
                    if (Regex.IsMatch(ostr, @"^"".*""$"))
                    {
                        // strip "".
                        ostr = ostr.Substring(1);
                        ostr = ostr.Substring(ostr.Length - 1);
                    }
                    var astr = AstOutputFile;
                    if (Regex.IsMatch(astr, @"^"".*""$"))
                    {
                        // strip "".
                        astr = astr.Substring(1);
                        astr = astr.Substring(astr.Length - 1);
                    }
                    string p = "\"" + ostr + "\\" + astr + "\"";
                    arguments.Add(p);
                }

                using (Process process = new Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = "dotnet.exe";
                    var a = String.Join(" ", arguments);
                    process.StartInfo.Arguments = a;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.ErrorDataReceived += HandleErrorDataReceived;
                    process.OutputDataReceived += HandleOutputDataReceived;
                    _buildMessages.Add(new BuildMessage(TraceLevel.Info,
                        "Executing command: \"" + process.StartInfo.FileName + "\" " + process.StartInfo.Arguments, "", 0, 0));
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
                if (e is TargetInvocationException && e.InnerException != null)
                    e = e.InnerException;

                _buildMessages.Add(new BuildMessage(TraceLevel.Error,
                    e.Message, "", 0, 0));
                success = false;
            }

            if (success)
            {
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
                        var ostr = OutputPath;
                        if (Regex.IsMatch(ostr, @"^"".*""$"))
                        {
                            // strip "".
                            ostr = ostr.Substring(1);
                            ostr = ostr.Substring(ostr.Length - 1);
                        }
                        var astr = AstOutputFile;
                        if (Regex.IsMatch(astr, @"^"".*""$"))
                        {
                            // strip "".
                            astr = astr.Substring(1);
                            astr = astr.Substring(astr.Length - 1);
                        }
                        string p = "\"" + ostr + "\\" + astr + "\"";
                        arguments.Add(p);
                    }
                    if (InitialTemplate != null)
                    {
                        arguments.Add("-s");
                        var str = InitialTemplate;
                        if (Regex.IsMatch(str, @"^"".*""$"))
                        {
                            // strip "".
                            str = str.Substring(1);
                            str = str.Substring(str.Length - 1);
                        }
                        str = "\"" + str + "\"";
                        arguments.Add(str);
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
                        var ostr = OutputPath;
                        if (Regex.IsMatch(ostr, @"^"".*""$"))
                        {
                            // strip "".
                            ostr = ostr.Substring(1);
                            ostr = ostr.Substring(ostr.Length - 1);
                        }
                        var astr = InitialTemplate;
                        if (Regex.IsMatch(astr, @"^"".*""$"))
                        {
                            // strip "".
                            astr = astr.Substring(1);
                            astr = astr.Substring(astr.Length - 1);
                        }

                        string p = "\"" + ostr + "\\" + Path.GetFileName(astr) + ".cs" + "\"";
                        arguments.Add(p);
                    }

                    using (Process process = new Process())
                    {
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.FileName = "dotnet.exe";
                        var a = String.Join(" ", arguments);
                        process.StartInfo.Arguments = a;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.RedirectStandardInput = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.ErrorDataReceived += HandleErrorDataReceived;
                        process.OutputDataReceived += HandleOutputDataReceived;
                        _buildMessages.Add(new BuildMessage(TraceLevel.Info,
                            "Executing command: \"" + process.StartInfo.FileName + "\" " + process.StartInfo.Arguments,
                            "", 0, 0));
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
                    if (e is TargetInvocationException && e.InnerException != null)
                        e = e.InnerException;

                    _buildMessages.Add(new BuildMessage(TraceLevel.Error,
                        e.Message, "", 0, 0));
                    success = false;
                }
            }

            foreach (BuildMessage message in _buildMessages)
            {
                ProcessBuildMessage(message);
            }

            if (!success) return success;

            _generatedCodeFiles.Add((ITaskItem)new TaskItem(OutputPath + "\\" + Path.GetFileName(InitialTemplate) + ".cs"));

            return success;
        }

        private void ProcessBuildMessage(BuildMessage message)
        {
            string errorCode;
            errorCode = Log.ExtractMessageCode(message.Message, out string logMessage);
            if (string.IsNullOrEmpty(errorCode))
            {
                if (message.Message.StartsWith("Executing command:", StringComparison.Ordinal) && message.Severity == TraceLevel.Info)
                {
                    // This is a known informational message
                    logMessage = message.Message;
                }
                else
                {
                    errorCode = "AC1000";
                    logMessage = "Unknown build error: " + message.Message;
                }
            }
            string subcategory = null;
            string helpKeyword = null;

            switch (message.Severity)
            {
                case TraceLevel.Error:
                    this.Log.LogError(logMessage);
                    break;
                case TraceLevel.Warning:
                    this.Log.LogWarning(logMessage);
                    break;
                case TraceLevel.Info:
                    this.Log.LogMessage(MessageImportance.Normal, logMessage);
                    break;
                case TraceLevel.Verbose:
                    this.Log.LogMessage(MessageImportance.Low, logMessage);
                    break;
            }
        }

        private void HandleErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            HandleErrorDataReceived(e.Data);
        }

        private void HandleErrorDataReceived(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;
            try
            {
                _buildMessages.Add(new BuildMessage(data));
            }
            catch (Exception ex)
            {
                _buildMessages.Add(new BuildMessage(ex.Message));
            }
        }

        private void HandleOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            HandleOutputDataReceived(e.Data);
        }

        private void HandleOutputDataReceived(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            try
            {
                _buildMessages.Add(new BuildMessage(data));
                return;
            }
            catch (Exception ex)
            {
                _buildMessages.Add(new BuildMessage(ex.Message));
            }
        }

    }
}