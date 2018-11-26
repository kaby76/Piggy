
namespace Piggy
{
    using System.IO;
    using System.Runtime.InteropServices;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using Microsoft.CSharp;
    using System.CodeDom.Compiler;
    using System.Text;
    using System.Reflection;

    public class Piggy
    {
        public Piggy() { }

        public static string copyright = @"
";
        public List<string> files = new List<string>();
        public string outputFile = string.Empty;
        public string specification = string.Empty;
        public string @namespace = string.Empty;
        public string libraryPath = string.Empty;
        public string prefixStrip = string.Empty;
        public string methodClassName = "Methods";
        public List<string> excludeFunctions = new List<string>();
        public string[] excludeFunctionsArray = null;
        public string add_after_using = "";
        public string calling_convention = "";
        public List<string> compiler_options = new List<string>();
        public bool ast = false;
        public List<List<SpecParserParser.TemplateContext>> templates = new List<List<SpecParserParser.TemplateContext>>();
        ErrorListener<IToken> listener = new ErrorListener<IToken>();
        IParseTree tree;
        public List<string> passes = new List<string>();

        [DllImport("ClangCode", EntryPoint = "ClangAddOption", CallingConvention = CallingConvention.StdCall)]
        private static extern void ClangAddOption([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler))] string @include);

        [DllImport("ClangCode", EntryPoint = "ClangAddFile", CallingConvention = CallingConvention.StdCall)]
        private static extern void ClangAddFile([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler))] string @file);

        [DllImport("ClangCode", EntryPoint = "ClangSerializeAst", CallingConvention = CallingConvention.StdCall)]
        private static unsafe extern IntPtr ClangSerializeAst();

        public static void Main(string[] args)
        {
            var p = new Piggy();
            p.Doit(args);
        }

        public unsafe void Doit(string[] args)
        {
            string temp_fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cpp";
            try
            {

                string full_path = Path.GetDirectoryName(Path.GetFullPath(typeof(Piggy).Assembly.Location))
                                   + Path.DirectorySeparatorChar;

                Regex re = new Regex(@"(?<switch>-{1,2}\S*)(?:[=:]?|\s+)(?<value>[^-\s].*?)?(?=\s+[-]|$)");
                List<KeyValuePair<string, string>> matches =
                    (from match in re.Matches(string.Join(" ", args)).Cast<Match>()
                        select new KeyValuePair<string, string>(match.Groups["switch"].Value,
                            match.Groups["value"].Value))
                    .ToList();

                foreach (KeyValuePair<string, string> match in matches)
                {
                    if (string.Equals(match.Key, "--o") || string.Equals(match.Key, "--output"))
                    {
                        outputFile = match.Value;
                    }

                    if (string.Equals(match.Key, "--s") || string.Equals(match.Key, "--spec"))
                    {
                        specification = match.Value;
                    }

                    if (string.Equals(match.Key, "--l") || string.Equals(match.Key, "--license"))
                    {
                        Console.WriteLine(copyright);
                    }

                    if (string.Equals(match.Key, "--ast"))
                    {
                        ast = true;
                    }
                }

                var errorList = new List<string>();

                if (!specification.Any())
                    errorList.Add("Error: No input C/C++ files provided. Use --file or --f");
                else
                {
                    // Parse specification file.
                    ICharStream stream = CharStreams.fromPath(specification);
                    ITokenSource lexer = new SpecLexer(stream);
                    ITokenStream tokens = new CommonTokenStream(lexer);
                    SpecParserParser parser = new SpecParserParser(tokens);
                    parser.BuildParseTree = true;
                    parser.AddErrorListener(listener);
                    tree = parser.spec();
                    if (listener.had_error)
                    {
                        System.Console.WriteLine(tree.GetText());
                        throw new Exception();
                    }
                    SpecListener printer = new SpecListener(this);
                    ParseTreeWalker.Default.Walk(printer, tree);
                }

                if (!files.Any())
                    errorList.Add("Error: No input C/C++ files provided. Use --file or --f");
                if (string.IsNullOrWhiteSpace(@namespace))
                    errorList.Add("Error: No namespace provided. Use --namespace or --n");
                if (string.IsNullOrWhiteSpace(outputFile))
                    errorList.Add("Error: No output file location provided. Use --output or --o");
                if (string.IsNullOrWhiteSpace(libraryPath))
                    errorList.Add("Error: No library path location provided. Use --libraryPath or --l");
                if (errorList.Any())
                {
                    Console.WriteLine("Usage: Piggy --specification [fileLocation] --output [output.cs] --ast");
                    Console.WriteLine("Note -- specification and output must not be null; ast is optional.");
                    foreach (var error in errorList)
                    {
                        Console.WriteLine(error);
                    }
                    throw new Exception();
                }

                if (excludeFunctions.Any())
                    excludeFunctionsArray = excludeFunctions.ToArray();

                // Set up file containing #includes of all the input files.
                StringBuilder str_builder = new StringBuilder();
                foreach (var file in files)
                {
                    str_builder.Append($"#include <{file}>");
                    str_builder.Append(Environment.NewLine);
                }

                File.WriteAllText(temp_fileName, str_builder.ToString());

                // Set up Clang front-end compilations in native code project "ClangCode".
                ClangAddFile(temp_fileName);

                // Set up clang options.
                foreach (var opt in compiler_options) ClangAddOption(opt);

                // serialize the AST for the desired input header files.
                IntPtr v = ClangSerializeAst();

                // Get back AST as string.
                string ast_result = Marshal.PtrToStringAnsi(v);
                if (ast)
                {
                    ast_result = ast_result.Replace("\n", "\r\n");
                    System.Console.WriteLine(ast_result);
                }

                // Parse ast using Antlr.
                ICharStream ast_stream = CharStreams.fromstring(ast_result);
                ITokenSource ast_lexer = new AstLexer(ast_stream);
                ITokenStream ast_tokens = new CommonTokenStream(ast_lexer);
                AstParserParser ast_parser = new AstParserParser(ast_tokens);
                ast_parser.BuildParseTree = true;
                ast_parser.AddErrorListener(listener);
                IParseTree ast_tree = ast_parser.ast();
                if (listener.had_error) throw new Exception();
                System.Console.WriteLine("Parsed successfully.");
                //System.Console.WriteLine("AST parsed");
                // Find and apply ordered regular expression templates until done.
                // Templates contain code, which has to be compiled and run.
                for (int pass = 0; pass < passes.Count; ++pass)
                {
                    string result = FindAndOutput(pass, ast_tree);
                    System.Console.WriteLine(result);
                }
            }
            finally
            {
                File.Delete(temp_fileName);
            }
        }


        string FindAndOutput(int pass, IParseTree ast)
        {
            List<SpecParserParser.TemplateContext> templates = this.templates[pass];
            TreeRegEx regex = new TreeRegEx(templates, ast.GetChild(0));
            regex.dfs_match();

            //foreach (var match in regex.matches)
            //{
            //    System.Console.WriteLine("Pattern " + TreeRegEx.sourceTextForContext(match.Value)
            //       + " matches tree node " + TreeRegEx.sourceTextForContext(match.Key));
            //}

            OutputEngine output = new OutputEngine();
            string @out = output.Generate(regex, ast);
            return @out;
        }
    }
}
