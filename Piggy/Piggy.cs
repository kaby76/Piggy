using org.antlr.symtab;

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
    using System.Text;
    using System.Reflection;

    public class Piggy
    {
        public Piggy() { }

        public static string _copyright = @"";
        public List<string> _clang_files = new List<string>();
        public string _specification = string.Empty;
        public List<string> _clang_options = new List<string>();
        public bool _display_ast = false;
        public Dictionary<string, List<SpecParserParser.TemplateContext>> _templates = new Dictionary<string, List<SpecParserParser.TemplateContext>>();
        public IParseTree _ast;
        public List<string> _passes = new List<string>();
        public string _current_pass;
        public Dictionary<IParseTree, MethodInfo> _code_blocks = new Dictionary<IParseTree, MethodInfo>();
        public object _cached_instance = null;
        public SymbolTable _symbol_table;
        public string _extends = "";
        public string _namespace = "";
        public string _header = "";
        public List<string> _referenced_assemblies = new List<string>();

        [DllImport("ClangCode", EntryPoint = "ClangAddOption", CallingConvention = CallingConvention.StdCall)]
        private static extern void ClangAddOption([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler))] string @include);

        [DllImport("ClangCode", EntryPoint = "ClangAddFile", CallingConvention = CallingConvention.StdCall)]
        private static extern void ClangAddFile([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler))] string @file);

        [DllImport("ClangCode", EntryPoint = "ClangSerializeAst", CallingConvention = CallingConvention.StdCall)]
        private static unsafe extern IntPtr ClangSerializeAst();

        public unsafe void Doit(string[] args)
        {
            string temp_fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cpp";
            ErrorListener<IToken> listener = new ErrorListener<IToken>();
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
                    if (string.Equals(match.Key, "--s") || string.Equals(match.Key, "--spec"))
                    {
                        _specification = match.Value;
                    }
                    if (string.Equals(match.Key, "--r"))
                    {
                        _referenced_assemblies.Add(match.Value);
                    }
                    if (string.Equals(match.Key, "--l") || string.Equals(match.Key, "--license"))
                    {
                        Console.WriteLine(_copyright);
                    }
                    if (string.Equals(match.Key, "--ast"))
                    {
                        _display_ast = true;
                    }
                }

                var errorList = new List<string>();

                if (!_specification.Any())
                    errorList.Add("Error: No input C/C++ files provided. Use --file or --f");

                else
                {
                    SpecFile file = new SpecFile(this);
                    file.ParseSpecFile(_specification);
                }

                if (!_clang_files.Any())
                    errorList.Add("Error: No input C/C++ files provided. Use --file or --f");
                if (errorList.Any())
                {
                    Console.WriteLine("Usage: Piggy spec-file-name [--_display_ast]");
                    Console.WriteLine("Note -- spec-file-name; _display_ast is optional. It can be in any order.");
                    foreach (var error in errorList)
                    {
                        Console.WriteLine(error);
                    }
                    throw new Exception();
                }

                // Set up file containing #includes of all the input files.
                StringBuilder str_builder = new StringBuilder();
                foreach (var file in _clang_files)
                {
                    str_builder.Append($"#include <{file}>");
                    str_builder.Append(Environment.NewLine);
                }

                File.WriteAllText(temp_fileName, str_builder.ToString());

                // Set up Clang front-end compilations in native code project "ClangCode".
                ClangAddFile(temp_fileName);

                // Set up clang options.
                foreach (var opt in _clang_options) ClangAddOption(opt);

                // serialize the AST for the desired input header files.
                IntPtr v = ClangSerializeAst();

                // Get back AST as string.
                string ast_result = Marshal.PtrToStringAnsi(v);
                if (_display_ast)
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
		        if (_display_ast)
		            Environment.Exit(0);
                AstSymtabBuilderListener ast_listener = new AstSymtabBuilderListener(ast_tree);
                ParseTreeWalker.Default.Walk(ast_listener, ast_tree);
                _ast = ast_tree;

                //System.Console.WriteLine("AST parsed");
                // Find and apply ordered regular expression templates until done.
                // Templates contain code, which has to be compiled and run.
                var output_engine = new OutputEngine(this);
                for (int pass = 0; pass < _passes.Count; ++pass)
                {
                    string result = FindAndOutput(output_engine, pass, ast_tree);
                    System.Console.WriteLine(result);
                }
            }
            finally
            {
                File.Delete(temp_fileName);
            }
        }

        string FindAndOutput(OutputEngine output, int pass, IParseTree ast)
        {
            string pass_name = this._passes[pass];
            List<SpecParserParser.TemplateContext> templates = this._templates[pass_name];
            TreeRegEx regex = new TreeRegEx(templates, ast.GetChild(0));
            regex.dfs_match();
#if DEBUGOUTPUT
            foreach (KeyValuePair<IParseTree, HashSet<IParseTree>> match in regex.matches)
            {
                System.Console.WriteLine("==========================");
                System.Console.WriteLine("Tree type " + match.Key.GetType());
                System.Console.WriteLine("Tree " + TreeRegEx.sourceTextForContext(match.Key));
                foreach (var m in match.Value)
                {
                    System.Console.WriteLine("Pattern type " + m.GetType());
                    System.Console.WriteLine("Pattern " + TreeRegEx.sourceTextForContext(m));
                }
            }
            System.Console.WriteLine("==========================");
#endif
            string @out = output.Generate(regex);
            return @out;
        }

        void SetUpSymbolTable()
        {
            // Create symbol table for AST.
            _symbol_table = new SymbolTable();
            AstSymtabBuilderListener listener = new AstSymtabBuilderListener(_ast);
            ParseTreeWalker.Default.Walk(listener, _ast);
        }

    }
}
