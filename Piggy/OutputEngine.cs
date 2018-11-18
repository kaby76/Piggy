using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using Microsoft.CSharp;

namespace Piggy
{
    public class OutputEngine
    {

        public string Generate(TreeRegEx re, IParseTree t)
        {
            StringBuilder builder = new StringBuilder();

            // Perform post-order traversal of AST, generate output for nodes
            // that have an associated pattern.

            var visited = new HashSet<IParseTree>();
            var stack = new Stack<IParseTree>();
            stack.Push(t);
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (visited.Contains(v))
                    continue;
                visited.Add(v);
                for (int i = v.ChildCount - 1; i >= 0; --i)
                {
                    var c = v.GetChild(i);
                    if (!visited.Contains(c))
                        stack.Push(c);
                }

                // Get associated pattern.
                re.matches.TryGetValue(v, out IParseTree p);

                if (p as SpecParserParser.TextContext != null)
                {
                    builder.Append(TreeRegEx.sourceTextForContext(v));
                }

                if (p as SpecParserParser.CodeContext != null)
                {
                    string code = @"
                using System;
                using System.IO;
                using System.Runtime.InteropServices;
                namespace First
                {
                    public class Program
                    {
                        public static string Gen()
                        {
                            return ""Hello world"";
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
                    parameters.GenerateExecutable = false;
                    parameters.CompilerOptions = "/unsafe";
                    CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);
                    if (results.Errors.HasErrors)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (CompilerError error in results.Errors)
                        {
                            sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                        }

                        System.Console.WriteLine(sb.ToString());
                        throw new InvalidOperationException(sb.ToString());
                    }

                    Assembly assembly = results.CompiledAssembly;
                    Type program = assembly.GetType("First.Program");
                    MethodInfo main = program.GetMethod("Main");
                    object[] a = new object[0];
                    var res = main.Invoke(null, a);
                }
            }
            return builder.ToString();
        }
    }
}
