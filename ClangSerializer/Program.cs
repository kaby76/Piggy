using System.Text.RegularExpressions;

namespace ClangSerializer
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using CommandLine;
    using System;
    using System.Runtime.InteropServices;
    using System.Linq;

    class Program
    {
        [DllImport("ClangCode", EntryPoint = "ClangAddOption", CallingConvention = CallingConvention.StdCall)]
        private static extern void ClangAddOption([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaller))] string @include);

        [DllImport("ClangCode", EntryPoint = "ClangAddFile", CallingConvention = CallingConvention.StdCall)]
        private static extern void ClangAddFile([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaller))] string @file);

        [DllImport("ClangCode", EntryPoint = "ClangSerializeAst", CallingConvention = CallingConvention.StdCall)]
        private static unsafe extern IntPtr ClangSerializeAst();

        class Options
        {
            [Option('c', "clang-option", Required = false, HelpText = "Clang option.")]
            public IEnumerable<string> ClangOptions { get; set; }

            [Option('f', "clang-file", Required = false, HelpText = "Clang input file.")]
            public IEnumerable<string> ClangFiles { get; set; }
        }

        static void Main(string[] args)
        {
            List<string> options = new List<string>();
            List<string> arguments = new List<string>();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    options = o.ClangOptions.Select(t => "-" + t).ToList();
                    arguments = o.ClangFiles.ToList();
                })
                .WithNotParsed(a =>
                {
                    System.Console.WriteLine(a);
                });

            // Set up Clang front-end compilations in native code project "ClangCode".
            foreach (var opt in arguments) ClangAddFile(opt);

            // Set up clang options.
            foreach (var opt in options) ClangAddOption(opt);

            // serialize the AST for the desired input header files.
            IntPtr v = ClangSerializeAst();

            string ast_result = Marshal.PtrToStringAnsi(v);

            string re2 = Regex.Replace(ast_result.ToString(), "\r([^\n])", "\r\n$1");
            string re3 = Regex.Replace(re2,"([^\r])\n", "$1\r\n");

            System.Console.WriteLine(re3);
        }
    }
}
