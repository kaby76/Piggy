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

        public string Generate(TreeRegEx re)
        {
            StringBuilder builder = new StringBuilder();
            var visited = new HashSet<IParseTree>();
            StackQueue<IParseTree> stack = new StackQueue<IParseTree>();
            StackQueue<List<IParseTree>> dfs_parent_chain = new StackQueue<List<IParseTree>>();
            stack.Push(re._ast);
            dfs_parent_chain.Push(new List<IParseTree>(){ });
            Dictionary<string, object> vars = new Dictionary<string, object>();
            while (stack.Count > 0)
            {
                var x = stack.Pop();
                var context = dfs_parent_chain.Pop();
                if (is_ast_node(x))
                {
                    if (visited.Contains(x)) continue;
                    visited.Add(x);
                    // Mutate the AST node to a pattern node if there is one.
                    re.matches.TryGetValue(x, out IParseTree pattern);
                    if (pattern != null)
                    {
			            System.Console.WriteLine("+++++");
			            System.Console.WriteLine("visiting x " + x.GetText().Substring(0, x.GetText().Length > 30 ? 30 : x.GetText().Length ));
                        //System.Console.WriteLine("looking for x " + TreeRegEx.sourceTextForContext(x));
                        System.Console.WriteLine("pattern " + TreeRegEx.sourceTextForContext(pattern));
			            System.Console.WriteLine("-----");
                        //stack.Push(pattern);
                        var en = (new List<IParseTree>()).Concat(context).Concat(new List<IParseTree>() {x, pattern}).ToList();
                        //dfs_parent_chain.Push(en);

                        // Immediately push children of pattern so as to avoid mutating the node back to an AST.
                        for (int i = pattern.ChildCount - 1; i >= 0; --i)
                        {
                            var c = pattern.GetChild(i);
                            stack.Push(c);
                            dfs_parent_chain.Push(en);
                        }
                    }
                    else
                    {
                        for (int i = x.ChildCount - 1; i >= 0; --i)
                        {
                            var c = x.GetChild(i);
                            if (!visited.Contains(c))
                            {
                                stack.Push(c);
                                dfs_parent_chain.Push((new List<IParseTree>()).Concat(context).Concat(new List<IParseTree>() { x }).ToList());
                            }
                        }
                    }
                }
                else if (is_spec_node(x))
                {
                    //System.Console.WriteLine("+sss+");
                    //System.Console.WriteLine("x " + TreeRegEx.sourceTextForContext(x));
                    //System.Console.WriteLine("-----");

                    if (x as SpecParserParser.TextContext != null)
                    {
                        string s = TreeRegEx.sourceTextForContext(x);
                        string s2 = s.Substring(1);
                        string s3 = s2.Substring(0, s2.Length - 1);
                        builder.Append(s3);
                    }
                    else if (x as SpecParserParser.CodeContext != null)
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
                            int top = context.Count - 1;
                            IParseTree con = null;
                            while (top >= 0)
                            {
                                con = context[top];
                                if (is_ast_node(con)) break;
                                top--;
                            }
                            a[1] = new Tree(re, re._ast, con);
                            a[2] = builder;
                            var res = main.Invoke(null, a);
                        }
                        finally
                        {
                            System.IO.File.Delete(fileName);
                        }
                    }
                    else if (x as SpecParserParser.BasicContext != null)
                    {
                        // For decl nodes, mutate back to AST.
                        // This can be tricky because it's many to one AST to pattern.
                        // Find possible AST nodes in matches.
                        List<KeyValuePair<IParseTree, IParseTree>> found_ast_match =
                            re.matches.Where(kvp => kvp.Value == x).ToList();

                        IParseTree look_for = null;
                        for (var jj = context.Count - 1; jj >= 0 && jj >= context.Count - 4; --jj)
                        {
                            var kk = context[jj];
                            if (kk.GetType().FullName.Contains("MoreContext"))
                            {
                                look_for = kk;
                                stack.Push(look_for);
                                dfs_parent_chain.Push((new List<IParseTree>()).Concat(context).Concat(new List<IParseTree>() { look_for }).ToList());
                                break;
                            }
                        }

                        if (look_for != null)
                        {

                        }
                        else
                        {
                            for (int i = x.ChildCount - 1; i >= 0; --i)
                            {
                                var c = x.GetChild(i);
                                if (!visited.Contains(c))
                                {
                                    stack.Push(c);
                                    dfs_parent_chain.Push((new List<IParseTree>()).Concat(context).Concat(new List<IParseTree>() { x }).ToList());
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = x.ChildCount - 1; i >= 0; --i)
                        {
                            var c = x.GetChild(i);
                            if (!visited.Contains(c))
                            {
                                stack.Push(c);
                                dfs_parent_chain.Push((new List<IParseTree>()).Concat(context).Concat(new List<IParseTree>() { x }).ToList());
                            }
                        }
                    }
                }
                else if (x as TerminalNodeImpl != null)
                    ;
                else
                    throw new Exception();
            }

            return builder.ToString();
        }
    }
}
