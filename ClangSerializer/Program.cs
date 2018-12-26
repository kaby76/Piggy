using System.Linq;

namespace ClangSerializer
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using CommandLine;
    using System;
    using System.Runtime.InteropServices;

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
            StringBuilder str_builder = new StringBuilder();
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

            var temp_file_name = Path.GetRandomFileName();
            File.WriteAllText(temp_file_name, str_builder.ToString());

            // Set up Clang front-end compilations in native code project "ClangCode".
            foreach (var opt in arguments) ClangAddFile(opt);

            // Set up clang options.
            foreach (var opt in options) ClangAddOption(opt);

            // serialize the AST for the desired input header files.
            IntPtr v = ClangSerializeAst();

            string ast_result = Marshal.PtrToStringAnsi(v);
            ast_result = ast_result.Replace("\n", "\r\n");
            System.Console.WriteLine(ast_result);
        }
    }
}
