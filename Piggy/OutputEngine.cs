using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.CSharp;

namespace Piggy
{
    public class OutputEngine
    {
        public bool is_ast_node(IParseTree x)
        {
            return (x as AstParserParser.AttrContext != null
                    || x as AstParserParser.AstContext != null
                    || x as AstParserParser.DeclContext != null
                    || x as AstParserParser.MoreContext != null);
        }

        public bool is_spec_node(IParseTree x)
        {
            return false
                   || x as SpecParserParser.Add_after_usingsContext != null
                   || x as SpecParserParser.AttrContext != null
                   || x as SpecParserParser.BasicContext != null
                   || x as SpecParserParser.Basic_rexpContext != null
                   || x as SpecParserParser.Calling_conventionContext != null
                   || x as SpecParserParser.Class_nameContext != null
                   || x as SpecParserParser.CodeContext != null
                   || x as SpecParserParser.Elementary_rexpContext != null
                   || x as SpecParserParser.Group_rexpContext != null
                   || x as SpecParserParser.ItemsContext != null
                   || x as SpecParserParser.MoreContext != null
                   || x as SpecParserParser.NamespaceContext != null
                   || x as SpecParserParser.Plus_rexpContext != null
                   || x as SpecParserParser.Prefix_stripContext != null
                   || x as SpecParserParser.RexpContext != null
                   || x as SpecParserParser.Simple_rexpContext != null
                   || x as SpecParserParser.SpecContext != null
                   || x as SpecParserParser.Star_rexpContext != null
                   || x as SpecParserParser.TemplateContext != null
                   || x as SpecParserParser.TextContext != null
                ;
        }

        public string Generate(TreeRegEx re, IParseTree t)
        {
            StringBuilder builder = new StringBuilder();
            var visited = new HashSet<IParseTree>();
            StackQueue<IParseTree> stack = new StackQueue<IParseTree>();
            stack.Push(t);
            Dictionary<string, object> vars = new Dictionary<string, object>();
            while (stack.Count > 0)
            {
                var x = stack.Pop();
                if (visited.Contains(x)) continue;
                visited.Add(x);

                // x could be either an AST node, or a pattern node.
                if (is_ast_node(x))
                {
                    re.matches.TryGetValue(x, out IParseTree p);
                    if (p != null)
                    {
                        System.Console.WriteLine("+++++");
                        System.Console.WriteLine("p " + TreeRegEx.sourceTextForContext(p));
                        System.Console.WriteLine("x " + TreeRegEx.sourceTextForContext(x));
                        System.Console.WriteLine("-----");
                        stack.Push(p);
                        continue;
                    }
                }
                else if (is_spec_node(x))
                {
                    if (x as SpecParserParser.TextContext != null)
                    {
                        string s = TreeRegEx.sourceTextForContext(x);
                        string s2 = s.Substring(1);
                        string s3 = s2.Substring(0, s2.Length - 1);
                        builder.Append(s3);
                    }

                    if (x as SpecParserParser.CodeContext != null)
                    {
                        string code = @"
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Piggy;
using System.Runtime.InteropServices;

namespace First
{
    public class Program
    {
        public static void Gen(
            Dictionary<string, object> vars,
            Piggy.Tree tree,
            StringBuilder result)
        {
" + TreeRegEx.sourceTextForContext(x) + @"
        }
    }
}
";
                        string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cs";
                        try
                        {
                            System.IO.File.WriteAllText(fileName, code);
                            CSharpCodeProvider provider = new CSharpCodeProvider();
                            CompilerParameters parameters = new CompilerParameters();
                            string full_path = System.IO.Path.GetFullPath(typeof(Piggy).Assembly.Location);
                            parameters.ReferencedAssemblies.Add(full_path);
                            // True - memory generation, false - external file generation
                            parameters.GenerateInMemory = true;
                            // True - exe file generation, false - dll file generation
                            parameters.GenerateExecutable = false;
                            parameters.CompilerOptions = "/unsafe";
                            parameters.IncludeDebugInformation = true;
                            CompilerResults results = provider.CompileAssemblyFromFile(parameters, new[] {fileName});
                            if (results.Errors.HasErrors)
                            {
                                StringBuilder sb = new StringBuilder();
                                foreach (CompilerError error in results.Errors)
                                {
                                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber,
                                        error.ErrorText));
                                }

                                System.Console.WriteLine("Compilation error for this code:");
                                System.Console.WriteLine(code);
                                System.Console.WriteLine(sb.ToString());
                                throw new InvalidOperationException(sb.ToString());
                            }

                            Assembly assembly = results.CompiledAssembly;
                            Type program = assembly.GetType("First.Program");
                            MethodInfo main = program.GetMethod("Gen");
                            object[] a = new object[3];
                            a[0] = vars;
                            var level = 0;
                            IParseTree par = null;
                            IParseTree c = x;
                            while (c != null)
                            {
                                foreach (var kvp in re.matches)
                                {
                                    if (kvp.Value == c)
                                    {
                                        par = kvp.Key;
                                        break;
                                    }
                                }

                                if (par != null) break;
                                re.parent.TryGetValue(c, out IParseTree pp);
                                c = pp;
                            }

                            a[1] = new Tree(re, t, par);
                            a[2] = builder;
                            var res = main.Invoke(null, a);
                        }
                        finally
                        {
                            System.IO.File.Delete(fileName);
                        }
                    }
                }
                else if (x as ParserRuleContext != null)
                    throw new Exception();

                for (int i = x.ChildCount - 1; i >= 0; --i)
                {
                    var c = x.GetChild(i);
                    if (!visited.Contains(c))
                        stack.Push(c);
                }
            }

            return builder.ToString();
        }
    }
}
