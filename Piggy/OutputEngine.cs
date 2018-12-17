﻿
// #define DEBUGOUTPUT

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Antlr4.Runtime.Tree;
using Microsoft.CSharp;

namespace Piggy
{
    public class OutputEngine
    {
        private Piggy _piggy;
        static Dictionary<string, object> vars = new Dictionary<string, object>();

        public OutputEngine(Piggy piggy)
        {
            _piggy = piggy;
        }

        public static bool is_ast_node(IParseTree x)
        {
            return (x as AstParserParser.AttrContext != null
                    || x as AstParserParser.AstContext != null
                    || x as AstParserParser.DeclContext != null
                    || x as AstParserParser.MoreContext != null);
        }

        public static bool is_spec_node(IParseTree x)
        {
            return false
                   || x as SpecParserParser.AttrContext != null
                   || x as SpecParserParser.BasicContext != null
                   || x as SpecParserParser.Id_or_star_or_emptyContext != null
                   || x as SpecParserParser.Basic_rexpContext != null
                   || x as SpecParserParser.CodeContext != null
                   || x as SpecParserParser.Elementary_rexpContext != null
                   || x as SpecParserParser.Group_rexpContext != null
                   || x as SpecParserParser.ItemsContext != null
                   || x as SpecParserParser.MoreContext != null
                   || x as SpecParserParser.Plus_rexpContext != null
                   || x as SpecParserParser.RexpContext != null
                   || x as SpecParserParser.Simple_rexpContext != null
                   || x as SpecParserParser.Simple_basicContext != null
                   || x as SpecParserParser.Kleene_star_basicContext != null
                   || x as SpecParserParser.SpecContext != null
                   || x as SpecParserParser.Star_rexpContext != null
                   || x as SpecParserParser.TemplateContext != null
                   || x as SpecParserParser.TextContext != null
                ;
        }

        public void CacheCompiledCodeBlocks()
        {
            // Create one file containing all code blocks in separate methods,
            // within one class, inherited from _extends.
            List<KeyValuePair<IParseTree, MethodInfo>> copy = _piggy._code_blocks.ToList();
            StringBuilder code = new StringBuilder();
            code.Append(
(_piggy._namespace != "" ? ("using " + _piggy._namespace + ";") : "")
+ @"
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
    public class Templates" + (_piggy._extends != "" ? " : " : "") + _piggy._extends + @"
    {
" + _piggy._header);
            int counter = 0;
            foreach (var t in copy)
            {
                var key = t.Key;
                var text = key.GetText();
                code.Append("public void Gen" + counter + @"(
            Dictionary<string, object> vars,
            Piggy.Tree tree,
            StringBuilder result)
        {
" + text + @"
        }
");
                counter++;
            }

            code.Append(@"
    }
}
");

            string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cs";
            try
            {
                System.IO.File.WriteAllText(fileName, code.ToString());
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();
                string full_path = System.IO.Path.GetFullPath(typeof(Piggy).Assembly.Location);
                full_path = System.IO.Path.GetDirectoryName(full_path);
                foreach (string f in System.IO.Directory.GetFiles(full_path))
                {
                    if (!f.EndsWith(".dll")) continue;
                    if (f.Contains("ClangCode")) continue;
                    parameters.ReferencedAssemblies.Add(f);
                }

                //foreach (var r in _piggy._referenced_assemblies)
                //{
                //    var aaa = System.Reflection.Assembly.LoadFile(r);
                //    aaa.GetTypes();
                //}
                //foreach (var r in _piggy._referenced_assemblies)
                //    parameters.ReferencedAssemblies.Add(r);
                // True - memory generation, false - external file generation
                parameters.GenerateInMemory = true;
                parameters.GenerateExecutable = false;
                //parameters.OutputAssembly = System.IO.Path.GetTempPath() + "MyAsm.dll";
                parameters.IncludeDebugInformation = true;
                CompilerResults results = provider.CompileAssemblyFromFile(parameters, new[]
                {
                    fileName
                    , @"C:\Users\Kenne\Documents\TemplateGenerator\TemplateGenerator\Class1.cs"
                });
                if (results.Errors.HasErrors)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (CompilerError error in results.Errors)
                    {
                        sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber,
                            error.ErrorText));
                    }
                    System.Console.WriteLine("Compilation error for this code:");
                    System.Console.WriteLine(code.ToString());
                    System.Console.WriteLine(sb.ToString());
                    throw new InvalidOperationException(sb.ToString());
                }
                Assembly assembly = results.CompiledAssembly;
                //Assembly asm = Assembly.LoadFrom(parameters.OutputAssembly);
                //try
                //{
                //    // load the assembly or type
                //    asm.GetTypes();
                //}
                //catch (Exception ex)
                //{
                //    if (ex is System.Reflection.ReflectionTypeLoadException)
                //    {
                //        var typeLoadException = ex as ReflectionTypeLoadException;
                //        var loaderExceptions = typeLoadException.LoaderExceptions;
                //    }
                //}

                Type program = assembly.GetType("First.Templates");
                counter = 0;
                _piggy._code_blocks = new Dictionary<IParseTree, MethodInfo>();
                foreach (var t in copy)
                {
                    var key = t.Key;
                    var text = key.GetText();
                    MethodInfo main = program.GetMethod("Gen" + counter++);
                    _piggy._code_blocks[key] = main;
                }
            }
            finally
            {
                //System.IO.File.Delete(fileName);
            }
        }

        public string Generate(TreeRegEx re)
        {
            CacheCompiledCodeBlocks();
            object o = null;
            if (_piggy._code_blocks.Any())
            {
                o = Activator.CreateInstance(_piggy._code_blocks.First().Value.DeclaringType);
            }

            StringBuilder builder = new StringBuilder();
            var visited = new HashSet<IParseTree>();
            StackQueue<IParseTree> stack = new StackQueue<IParseTree>();
            StackQueue<List<IParseTree>> dfs_parent_chain = new StackQueue<List<IParseTree>>();
            stack.Push(re._ast);
            dfs_parent_chain.Push(new List<IParseTree>(){ });
            while (stack.Count > 0)
            {
                var x = stack.Pop();
                var context = dfs_parent_chain.Pop();
                if (is_ast_node(x))
                {
                    if (visited.Contains(x)) continue;
                    visited.Add(x);

                    re.matches.TryGetValue(x, out HashSet<IParseTree> v);
                    IParseTree pattern = v?.FirstOrDefault();
                    int count = v == null ? 0 : v.Count;
                    int i = pattern == null ? 0 : pattern.ChildCount - 1;
                    List<IParseTree> after = new List<IParseTree>();

                    // Interleave children of pattern with children of AST.
                    int ci = 0;
                    for (int ai = 0; ai < x.ChildCount; ++ai)
                    {
                        // Get child x[ai];
                        var c = x.GetChild(ai);
                        re.matches.TryGetValue(c, out HashSet<IParseTree> vc);
                        IParseTree pc = vc?.FirstOrDefault();
                        int countc = vc == null ? 0 : vc.Count;
                        int ic = pc == null ? 0 : pc.ChildCount - 1;

                        if (pattern != null && pc != null)
                        {
#if DEBUGOUTPUT
                            System.Console.WriteLine("------------- types");
                            System.Console.WriteLine("x type " + x.GetType());
                            System.Console.WriteLine("c type " + c.GetType());
                            System.Console.WriteLine("p type " + pattern.GetType());
                            System.Console.WriteLine("pc type " + pc.GetType());
                            System.Console.WriteLine("------------- values");
                            System.Console.WriteLine("x value " + TreeRegEx.sourceTextForContext(x));
                            System.Console.WriteLine("c value " + TreeRegEx.sourceTextForContext(c));
                            System.Console.WriteLine("p value " + TreeRegEx.sourceTextForContext(pattern));
                            System.Console.WriteLine("pc value " + TreeRegEx.sourceTextForContext(pc));
#endif
                            for (; ci < pattern.ChildCount; ++ci)
                            {
                                var cp = pattern.GetChild(ci);
                                // We only care if the pattern node is directly an
                                // attr or code node. Otherwise, it will be handled
                                // by dfs of the child.
#if DEBUGOUTPUT
                                System.Console.WriteLine("cp type " + cp.GetType());
                                System.Console.WriteLine("cp value " + TreeRegEx.sourceTextForContext(cp));
#endif
                                if (cp as SpecParserParser.MoreContext != null
                                    && cp.ChildCount == 1
                                    && (cp.GetChild(0) as SpecParserParser.TextContext != null
                                        || cp.GetChild(0) as SpecParserParser.CodeContext != null))
                                {
                                    after.Insert(0, cp);
                                }
                                bool exit_loop = false;
                                foreach (var ii in vc)
                                {
                                    if (cp == ii)
                                    {
                                        exit_loop = true;
                                        break;
                                    }
                                }
                                if (exit_loop) break;
                            }
                        }
                        
                        after.Insert(0, c);
                    }

                    foreach (var al in after)
                    {
                        stack.Push(al);
                        dfs_parent_chain.Push((new List<IParseTree>()).Concat(context)
                            .Concat(new List<IParseTree>() {x}).ToList());
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
                        string s2 = s.Substring(2);
                        string s3 = s2.Substring(0, s2.Length - 2);
                        builder.Append(s3);
                    }
                    else if (x as SpecParserParser.CodeContext != null)
                    {
                        // Verify:
                        // 1. This must have an AST context.
                        //   => Easy to know because the dfs parent list
                        //      is retained.
                        int top = context.Count - 1;
                        IParseTree con = null;
                        while (top >= 0)
                        {
                            con = context[top];
#if DEBUGOUTPUT
                            System.Console.WriteLine("con " + TreeRegEx.sourceTextForContext(con));
                            System.Console.WriteLine("-----");
#endif
                            if (is_ast_node(con)) break;
                            top--;
                        }
                        if (top < 0) continue;

                        // 2. It must be derived from a matching parent.
                        //   => Look up the dfs parent list, since this is the
                        //      combination of the _display_ast and pattern trees.
                        //      Do not consider this node if the AST forms a
                        //      different group.

                        try
                        {
                            MethodInfo main = _piggy._code_blocks[x];
                            object[] a = new object[3];
                            a[0] = vars;
                            a[1] = new Tree(re.parent, re._ast, con);
                            a[2] = builder;
                            var res = main.Invoke(o, a);
                        }
                        finally
                        {
                        }
                    }
                    else if (x as SpecParserParser.BasicContext != null)
                    {
                        // For decl nodes, mutate back to AST.
                        // This can be tricky because it's many to one AST to pattern.
                        // Find possible AST nodes in matches.
                        List<KeyValuePair<IParseTree, HashSet<IParseTree>>> found_ast_match =
                            re.matches.Where(kvp =>
                            {
                                var y = kvp.Value;
                                return y.Contains(x);
                            }).ToList();

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
