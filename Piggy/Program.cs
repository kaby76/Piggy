using PiggyGenerator;
using CommandLine;

namespace ConsoleApp1
{
    public class Program
    {
        class Options
        {
            [Option('a', "clang-ast-file", Required = false, HelpText = "Clang ast input file.")]
            public string ClangFile { get; set; }

            [Option('s', "piggy-spec-file", Required = true, HelpText = "Piggy spec input file.")]
            public string PiggyFile { get; set; }

            [Option('k', "keep-intermediate-file", Required = false, HelpText = "Keep the intermediate C# file for debugging.")]
            public bool KeepFile { get; set; }
        }


        public static void Main(string[] args)
        {
            var p = new Piggy();

            string ast_file = null;
            string spec_file = null;
            bool keep_file = false;
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    ast_file = o.ClangFile;
                    spec_file = o.PiggyFile;
                    keep_file = o.KeepFile;
                })
                .WithNotParsed(a =>
                {
                    System.Console.WriteLine(a);
                });

            p.Doit(ast_file, spec_file, keep_file);
        }
    }
}
