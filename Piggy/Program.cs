using System.Runtime.InteropServices;

namespace Piggy
{
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

    public class Program
    {
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

        [DllImport("ClangCode", EntryPoint = "SearchSetPattern", CallingConvention = CallingConvention.StdCall)]
        private static extern void SearchSetPattern([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler))] string @pattern);

        [DllImport("ClangCode", EntryPoint = "SearchAddCompilerOption", CallingConvention = CallingConvention.StdCall)]
        private static extern void SearchAddCompilerOption([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler))] string @include);

        [DllImport("ClangCode", EntryPoint = "SearchAddFile", CallingConvention = CallingConvention.StdCall)]
        private static extern void SearchAddFile([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler))] string @file);

        [DllImport("ClangCode", EntryPoint = "Search", CallingConvention = CallingConvention.StdCall)]
        private static unsafe extern void** Search();

        [DllImport("ClangCode", EntryPoint = "xxx", CallingConvention = CallingConvention.StdCall)]
        private static extern int xxx();

        public static void Main(string[] args)
        {
            var p = new Program();
            p.Doit(args);
        }

        public void Doit(string[] args)
        {
            Regex re = new Regex(@"(?<switch>-{1,2}\S*)(?:[=:]?|\s+)(?<value>[^-\s].*?)?(?=\s+[-]|$)");
            List<KeyValuePair<string, string>> matches = (from match in re.Matches(string.Join(" ", args)).Cast<Match>()
                                                          select new KeyValuePair<string, string>(match.Groups["switch"].Value, match.Groups["value"].Value))
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

            // Parse specification file.
            ICharStream stream = CharStreams.fromPath(specification);
            ITokenSource lexer = new SpecLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            SpecParserParser parser = new SpecParserParser(tokens);
            parser.BuildParseTree = true;
            var listener = new ErrorListener<IToken>();
            parser.AddErrorListener(listener);
            IParseTree tree = parser.spec();
            if (listener.had_error) throw new Exception();

            SpecListener printer = new SpecListener(this);
            ParseTreeWalker.Default.Walk(printer, tree);

            var errorList = new List<string>();
            if (!files.Any())
            {
                errorList.Add("Error: No input C/C++ files provided. Use --file or --f");
            }

            if (string.IsNullOrWhiteSpace(@namespace))
            {
                errorList.Add("Error: No namespace provided. Use --namespace or --n");
            }

            if (string.IsNullOrWhiteSpace(outputFile))
            {
                errorList.Add("Error: No output file location provided. Use --output or --o");
            }

            if (string.IsNullOrWhiteSpace(libraryPath))
            {
                errorList.Add("Error: No library path location provided. Use --libraryPath or --l");
            }

            if (errorList.Any())
            {
                Console.WriteLine("Usage: Piggy --specification [fileLocation] --output [output.cs] --ast");
                Console.WriteLine("Note -- specification and output must not be null; ast is optional.");
                foreach (var error in errorList)
                {
                    Console.WriteLine(error);
                }
            }

            if (excludeFunctions.Any())
            {
                excludeFunctionsArray = excludeFunctions.ToArray();
            }


            foreach (var file in files)
            {
                SearchAddFile(file);
            }
            foreach (var opt in compiler_options)
            {
                SearchAddCompilerOption(opt);
            }

            SearchSetPattern("enumDecl()");
            unsafe
            {
                void ** v = Search();
            }

            string code = @"
                using System;
                namespace First
                {
                    public class Program
                    {
                        [DllImport(""ClangCode"", EntryPoint = ""xxx"", CallingConvention = CallingConvention.StdCall)]
                        private static extern int xxx();

                        public static void Main()
                        {
                        }
                    }
                }
            ";

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            // parameters.ReferencedAssemblies.Add("System.Drawing.dll");
            // True - memory generation, false - external file generation
            parameters.GenerateInMemory = true;
            // True - exe file generation, false - dll file generation
            parameters.GenerateExecutable = true;
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);
            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();
                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }
                throw new InvalidOperationException(sb.ToString());
            }
            Assembly assembly = results.CompiledAssembly;
            Type program = assembly.GetType("First.Program");
            MethodInfo main = program.GetMethod("Main");
            main.Invoke(null, null);
        }
    }
}
