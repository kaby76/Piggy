namespace ClangSerializer
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using CommandLine;
    using System;
    using System.Runtime.InteropServices;
    using System.Linq;
    using System.Text.RegularExpressions;

    class Program
    {
        [DllImport("ClangCode", EntryPoint = "ClangAddOption", CallingConvention = CallingConvention.StdCall)]
        private static extern void ClangAddOption([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaller))] string @include);

        [DllImport("ClangCode", EntryPoint = "ClangAddFile", CallingConvention = CallingConvention.StdCall)]
        private static extern void ClangAddFile([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaller))] string @file);

        [DllImport("ClangCode", EntryPoint = "ClangSerializeAst", CallingConvention = CallingConvention.StdCall)]
        private static unsafe extern IntPtr ClangSerializeAst();

        [DllImport("ClangCode", EntryPoint = "ClangSetPackedAst", CallingConvention = CallingConvention.StdCall)]
        private static unsafe extern void ClangSetPackedAst(bool packed_ast);

        class Options
        {
            [Option('c', "clang-option", Required = false, HelpText = "Clang option.")]
            public IEnumerable<string> ClangOptions { get; set; }

            [Option('f', "clang-files", Required = false, HelpText = "Clang C input files.")]
            public IEnumerable<string> ClangFiles { get; set; }

            [Option('o', "ast-out-file", Required = false, HelpText = "AST output file.")]
            public string AstOutFile { get; set; }

            [Option('p', "packed-ast", Required = false, HelpText = "Set for tightly packed/terse representation of AST.")]
            public bool PackedAst { get; set; }
        }

        static void Main(string[] args)
        {
            List<string> options = new List<string>();
            List<string> arguments = new List<string>();
            string ast_output_file = null;
            bool packed_ast = false;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    options = o.ClangOptions.Select(t => "-" + t).ToList();
                    arguments = o.ClangFiles.ToList();
                    ast_output_file = o.AstOutFile;
                    packed_ast = o.PackedAst;
                })
                .WithNotParsed(a =>
                {
                    System.Console.WriteLine(a);
                });


            // Set up Clang front-end compilations in native code project "ClangCode".
            foreach (var opt in arguments) ClangAddFile(opt);

            // Set up clang options.
            foreach (var opt in options) ClangAddOption(opt);

            ClangSetPackedAst(packed_ast);

            // serialize the AST for the desired input header files.
            IntPtr v = ClangSerializeAst();

            System.Console.WriteLine("Info: Command line args " + string.Join(" ", args));
            System.Console.WriteLine("Info: arguments " + string.Join(" ", arguments));
            System.Console.WriteLine("Info: options " + string.Join(" ", options));
            System.Console.WriteLine("Info: ast_output_file " + ast_output_file);
            System.Console.WriteLine("Info: packed_ast " + packed_ast);

            if (v == IntPtr.Zero)
            {
                Environment.ExitCode = 1;
                return;
            }
            string ast_result = Marshal.PtrToStringAnsi(v);

            string re2 = Regex.Replace(ast_result.ToString(), "\r([^\n])", "\r\n$1");
            string re3 = Regex.Replace(re2,"([^\r])\n", "$1\r\n");

            if (ast_output_file == null || ast_output_file == "")
                System.Console.WriteLine(re3);
            else
            {
                StringBuilder sbb = new StringBuilder();
                using (StringWriter writer = new StringWriter(sbb))
                {
                    System.IO.File.WriteAllText(ast_output_file, re3);
                }
            }
        }
    }
}
