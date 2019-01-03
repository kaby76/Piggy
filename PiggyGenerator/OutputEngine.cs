namespace PiggyGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Antlr4.Runtime.Tree;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Formatting;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Options;
    using Microsoft.CodeAnalysis.Text;
    using PiggyRuntime;

    public class OutputEngine
    {
        private Piggy _piggy;

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
                   || x as SpecParserParser.Basic_rexpContext != null
                   || x as SpecParserParser.ClangContext != null
                   || x as SpecParserParser.Clang_fileContext != null
                   || x as SpecParserParser.Clang_optionContext != null
                   || x as SpecParserParser.CodeContext != null
                   || x as SpecParserParser.Elementary_rexpContext != null
                   || x as SpecParserParser.ExtendsContext != null
                   || x as SpecParserParser.Group_rexpContext != null
                   || x as SpecParserParser.HeaderContext != null
                   || x as SpecParserParser.Id_or_star_or_emptyContext != null
                   || x as SpecParserParser.Kleene_star_basicContext != null
                   || x as SpecParserParser.MoreContext != null
                   || x as SpecParserParser.PassContext != null
                   || x as SpecParserParser.PatternContext != null
                   || x as SpecParserParser.Plus_rexpContext != null
                   || x as SpecParserParser.RexpContext != null
                   || x as SpecParserParser.Simple_rexpContext != null
                   || x as SpecParserParser.Simple_basicContext != null
                   || x as SpecParserParser.SpecContext != null
                   || x as SpecParserParser.Star_rexpContext != null
                   || x as SpecParserParser.TemplateContext != null
                   || x as SpecParserParser.TextContext != null
                   || x as SpecParserParser.UsingContext != null
                ;
        }

        private bool IsThisAppNetCore()
        {
            var trustedAssembliesPaths = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            return trustedAssembliesPaths != null;
        }

        public void ReferencedFrameworkAssemblies(List<MetadataReference> all_references)
        {
            List<string> result = new List<string>();
            if (IsThisAppNetCore())
            {
                var trustedAssembliesPaths = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
                var s = trustedAssembliesPaths as string;
                var l = s.Split(Path.PathSeparator);
                result = l.ToList();
                result = result.Where(x => !x.Contains("System.Private.CoreLib")).ToList();
            }
            foreach (var r in result)
            {
                var jj = Assembly.LoadFrom(r);
                all_references.Add(MetadataReference.CreateFromFile(jj.Location));
            }
        }

        private void FixUpMetadataReferences(List<MetadataReference> all_references, Type type)
        {
            var stack = new Stack<Assembly>();
            Assembly a = type.Assembly;
            HashSet<Assembly> visited = new HashSet<Assembly>();
            stack.Push(a);
            while (stack.Any())
            {
                var t = stack.Pop();
                if (visited.Contains(t)) continue;
                visited.Add(t);
                if (t.Location.Contains("netstandard.dll"))
                {
                    ReferencedFrameworkAssemblies(all_references);
                }
                all_references.Add(MetadataReference.CreateFromFile(t.Location));
                foreach (var r in a.GetReferencedAssemblies())
                {
                    AssemblyName q = r;
                    var jj = Assembly.Load(q);
                    stack.Push(jj);
                }
            }
        }

        public void CompileTemplates()
        {
            // Create one file containing all types and decls:
            // 1) Create a class for each "template", subclassing based on extension if given
            // in the spec.
            // 2) All code block are place in separate methods, within the enclosing class/template.
            // 3) Make sure to set up the constructor for the class if one.
            string @namespace = "CompiledTemplates";
            StringBuilder code = new StringBuilder();
            code.Append(@"
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PiggyRuntime;
using System.Runtime.InteropServices;
using org.antlr.symtab;

namespace " + @namespace + @"
{
");

            Dictionary<IParseTree, string> gen_named_code_blocks = new Dictionary<IParseTree, string>();
            foreach (var template in _piggy._templates)
            {
                List<KeyValuePair<IParseTree, MethodInfo>> copy = _piggy._code_blocks.ToList();
                code.Append(@"
    public class " + template.TemplateName + " : " + (template.Extends != null ? template.Extends : "Template") + @"
{
");
                // Emit the header code of this template.
                foreach (var c in template.Headers)
                    code.Append(c);

                // Emit the initializer code of this template.
                // Here, we are using the class constructor.
                code.AppendLine(@"public " + template.TemplateName + "(){");
                foreach (var c in template.Initializations)
                    code.Append(c);
                code.AppendLine("}");

                Dictionary<IParseTree, string> collected_code = new Dictionary<IParseTree, string>();
                foreach (var p in template.Passes)
                {
                    foreach (Pattern pt in p.Patterns)
                    {
                        Dictionary<IParseTree, string> c = pt.CollectCode();
                        foreach (var x in c) collected_code.Add(x.Key, x.Value);
                    }
                }

                // So, for every damn code block, output a "Gen#()" method.
                // And, make sure to associate the name of the method with the tree node
                // so we can do fast look ups.
                int counter = 0;
                foreach (var t in collected_code)
                {
                    var key = t.Key;
                    var text = t.Value;
                    var method_name = "Gen" + counter++;
                    gen_named_code_blocks[key] = method_name;
                    code.Append("public void " + method_name + @"(
            PiggyRuntime.Tree tree,
            StringBuilder result)
        {
" + text + @"
        }
");
                }

                code.Append(@"
    }
");
            }

            code.Append(@"
}
");

            try
            {
                // After generating the code, let's write reformat it.
                string formatted_source_code;
                string formatted_source_code_path = @"c:\temp\generated_templates.cs";
                {
                    var workspace = new AdhocWorkspace();
                    string projectName = "FormatTemplates";
                    ProjectId projectId = ProjectId.CreateNewId();
                    VersionStamp versionStamp = VersionStamp.Create();
                    ProjectInfo helloWorldProject = ProjectInfo.Create(projectId, versionStamp, projectName,
                        projectName, LanguageNames.CSharp);
                    SourceText sourceText = SourceText.From(code.ToString());
                    Project newProject = workspace.AddProject(helloWorldProject);
                    Document newDocument = workspace.AddDocument(newProject.Id, "Program.cs", sourceText);
                    OptionSet options = workspace.Options;
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, false);
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, false);
                    SyntaxNode syntaxRoot = newDocument.GetSyntaxRootAsync().Result;
                    SyntaxNode formattedNode = Formatter.Format(syntaxRoot, workspace, options);
                    StringBuilder sbb = new StringBuilder();
                    using (StringWriter writer = new StringWriter(sbb))
                    {
                        formattedNode.WriteTo(writer);
                        formatted_source_code = writer.ToString();
                        System.IO.File.WriteAllText(formatted_source_code_path, formatted_source_code);
                    }
                }

                // With template classes generated, let's compile them.
                {
                    string assemblyName = Path.GetRandomFileName();
                    string symbolsName = Path.ChangeExtension(assemblyName, "pdb");

                    SourceText sourceText = SourceText.From(formatted_source_code, Encoding.UTF8);
                    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
                        sourceText,
                        new CSharpParseOptions(),
                        path: formatted_source_code_path);
                    var syntaxRootNode = syntaxTree.GetRoot() as CSharpSyntaxNode;
                    var encoded = CSharpSyntaxTree.Create(syntaxRootNode,
                        null, formatted_source_code_path, Encoding.UTF8);

                    //AdhocWorkspace workspace = new Microsoft.CodeAnalysis.AdhocWorkspace();
                    //string projectName = "CompileTemplates";
                    //ProjectId projectId = ProjectId.CreateNewId();
                    //VersionStamp versionStamp = VersionStamp.Create();
                    //ProjectInfo helloWorldProject = ProjectInfo.Create(projectId, versionStamp, projectName,
                    //    projectName, LanguageNames.CSharp);
                    //Project newProject = workspace.AddProject(helloWorldProject);
                    //Document newDocument = workspace.AddDocument(newProject.Id, formatted_source_code_path, sourceText);
                    //SyntaxNode syntaxRoot = newDocument.GetSyntaxRootAsync().Result;
                    //var encoded = syntaxRoot.SyntaxTree;

                    List<MetadataReference> all_references = new List<MetadataReference>();
                    FixUpMetadataReferences(all_references, typeof(PiggyRuntime.Template));
                    FixUpMetadataReferences(all_references, typeof(object));

                    CSharpCompilation compilation = CSharpCompilation.Create(
                        assemblyName,
                        syntaxTrees: new[] {encoded},
                        references: all_references.ToArray(),
                        options: new CSharpCompilationOptions(
                            OutputKind.DynamicallyLinkedLibrary)
                            .WithOptimizationLevel(OptimizationLevel.Debug)
                            .WithPlatform(Platform.AnyCpu)
                    );
                    Assembly assembly = null;
                    using (var assemblyStream = new MemoryStream())
                    using (var symbolsStream = new MemoryStream())
                    {
                        var emit_options = new EmitOptions(
                            debugInformationFormat: DebugInformationFormat.PortablePdb,
                            pdbFilePath: symbolsName
                        );

                        var embeddedTexts = new List<EmbeddedText>
                        {
                            EmbeddedText.FromSource(formatted_source_code_path, sourceText),
                        };

                        EmitResult result = compilation.Emit(
                            peStream: assemblyStream,
                            pdbStream: symbolsStream,
                            embeddedTexts: embeddedTexts,
                            options: emit_options);

                        if (!result.Success)
                        {
                            IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                                diagnostic.IsWarningAsError ||
                                diagnostic.Severity == DiagnosticSeverity.Error);

                            foreach (Diagnostic diagnostic in failures)
                            {
                                Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                            }
                            throw new Exception();
                        }
                        else
                        {
                            assemblyStream.Seek(0, SeekOrigin.Begin);
                            symbolsStream.Seek(0, SeekOrigin.Begin);
                            assembly = Assembly.Load(assemblyStream.ToArray(), symbolsStream.ToArray());
                        }
                    }

                    _piggy._code_blocks = new Dictionary<IParseTree, MethodInfo>();
                    //Assembly assembly = results.CompiledAssembly;
                    foreach (Template template in _piggy._templates)
                    {
                        var class_name = @namespace + "." + template.TemplateName;
                        Type template_type = assembly.GetType(class_name);
                        template.Type = template_type;
                        foreach (Pass pass in template.Passes)
                        {
                            foreach (Pattern pattern in pass.Patterns)
                            {
                                Dictionary<IParseTree, string> x = pattern.CollectCode();
                                foreach (KeyValuePair<IParseTree, string> kvp in x)
                                {
                                    IParseTree key = kvp.Key;
                                    string name = gen_named_code_blocks[key];
                                    MethodInfo method_info = template_type.GetMethod(name);
                                    if (method_info == null)
                                        throw new Exception("Can't find method_info for " + class_name + "." + name);
                                    _piggy._code_blocks[key] = method_info;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                //System.IO.File.Delete(fileName);
            }
        }

        static Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public void PatternMatchingEngine(StringBuilder builder, TreeRegEx re)
        {
            var visited = new HashSet<IParseTree>();
            StackQueue<IParseTree> stack = new StackQueue<IParseTree>();
            StackQueue<List<IParseTree>> dfs_parent_chain = new StackQueue<List<IParseTree>>();
            stack.Push(re._ast);
            dfs_parent_chain.Push(new List<IParseTree>() { });
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
                        var c = x.GetChild(ai);
                        re.matches.TryGetValue(c, out HashSet<IParseTree> vc);
                        IParseTree pc = vc?.FirstOrDefault();
                        int countc = vc == null ? 0 : vc.Count;
                        int ic = pc == null ? 0 : pc.ChildCount - 1;

                        if (pattern != null && pc != null)
                        {
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
                            .Concat(new List<IParseTree>() { x }).ToList());
                    }
                }
                else if (is_spec_node(x))
                {
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
                            Type type = re._current_type;
                            object instance = re._instance;
                            object[] a = new object[] { new Tree(re.parent, re._ast, con), builder };
                            var res = main.Invoke(instance, a);
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
        }

        public void OutputMatches(StringBuilder builder, TreeRegEx re)
        {
            var visited = new HashSet<IParseTree>();
            StackQueue<IParseTree> stack = new StackQueue<IParseTree>();
            stack.Push(re._ast);
            while (stack.Count > 0)
            {
                var x = stack.Pop();
                if (is_ast_node(x))
                {
                    if (visited.Contains(x)) continue;
                    visited.Add(x);
                    re.matches.TryGetValue(x, out HashSet<IParseTree> v);
                    if (v != null)
                    {
                        System.Console.WriteLine(TreeRegEx.sourceTextForContext(x));
                    }
                    else
                        for (int i = x.ChildCount - 1; i >= 0; --i)
                        {
                            var c = x.GetChild(i);
                            if (!visited.Contains(c))
                            {
                                stack.Push(c);
                            }
                        }
                }
                else if (x as TerminalNodeImpl != null)
                    ;
                else
                    throw new Exception();
            }
        }

        private List<Pass> GetAllPassesNamed(Template template, string pass_name)
        {
            List<Pass> collection = new List<Pass>();
            var possible = template.Passes.Find(p => p.Name == pass_name);
            if (possible != null) collection.Add(possible);
            if (template.Extends != null)
            {
                var extension = template.Extends;
                var extension_template = _piggy._templates.Find(t => t.TemplateName == extension);
                if (extension_template == null) throw new Exception("Cannot find template " + extension);
                var more = GetAllPassesNamed(extension_template, pass_name);
                collection.AddRange(more);
            }
            return collection;
        }

        public string Run(bool just_find)
        {
            StringBuilder combined_result = new StringBuilder();
            CompileTemplates();
            // Create types for passes referenced.
            foreach (var full_pass_name in _piggy._application.OrderedPasses)
            {
                var pass_name_regex = new Regex("^(?<template_name>[^.]+)[.](?<pass_name>.*)$");
                var match = pass_name_regex.Match(full_pass_name);
                if (!match.Success) throw new Exception("template.pass " + full_pass_name + " does not exist.");
                var template_name = match.Groups["template_name"].Value;
                var pass_name = match.Groups["pass_name"].Value;
                var template = _piggy._templates.Find(t => t.TemplateName == template_name);
                var type = template.Type;
                _instances.TryGetValue(type, out object i);
                if (i == null)
                {
                    _instances[type] = Activator.CreateInstance(type);
                }
            }
            var app = _piggy._application;
            foreach (var full_pass_name in app.OrderedPasses)
            {
                // Separate the pass name into "template name" "." "pass name"
                var pass_name_regex = new Regex("^(?<template_name>[^.]+)[.](?<pass_name>.*)$");
                var match = pass_name_regex.Match(full_pass_name);
                if (!match.Success) throw new Exception("template.pass " + full_pass_name + " does not exist.");
                var template_name = match.Groups["template_name"].Value;
                var pass_name = match.Groups["pass_name"].Value;
                var template = _piggy._templates.Find(t => t.TemplateName == template_name);
                List<Pass> passes = GetAllPassesNamed(template, pass_name);
                TreeRegEx regex = new TreeRegEx(_piggy, passes, _instances[template.Type]);
                regex.dfs_match();
                if (just_find) OutputMatches(combined_result, regex);
                else PatternMatchingEngine(combined_result, regex);
            }

            if (just_find)
            {
                return combined_result.ToString();
            }
            else
            {
                string sa = combined_result.ToString();
                var workspace = new AdhocWorkspace();
                string projectName = "HelloWorldProject";
                ProjectId projectId = ProjectId.CreateNewId();
                VersionStamp versionStamp = VersionStamp.Create();
                ProjectInfo helloWorldProject = ProjectInfo.Create(projectId, versionStamp, projectName, projectName,
                    LanguageNames.CSharp);
                SourceText sourceText = SourceText.From(sa);
                Project newProject = workspace.AddProject(helloWorldProject);
                Document newDocument = workspace.AddDocument(newProject.Id, "Program.cs", sourceText);
                OptionSet options = workspace.Options;
                options = options
                        .WithChangedOption(CSharpFormattingOptions.IndentBlock, true)
                        .WithChangedOption(CSharpFormattingOptions.IndentBraces, false)
                        .WithChangedOption(CSharpFormattingOptions.IndentSwitchCaseSection, true)
                        .WithChangedOption(CSharpFormattingOptions.IndentSwitchCaseSectionWhenBlock, true)
                        .WithChangedOption(CSharpFormattingOptions.IndentSwitchSection, true)
                        .WithChangedOption(CSharpFormattingOptions.NewLineForElse, true)
                        .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, true)
                        .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, true)
                    ;
                var syntaxRoot = newDocument.GetSyntaxRootAsync().Result;
                SyntaxNode formattedNode = Formatter.Format(syntaxRoot, workspace, options);
                StringBuilder sb = new StringBuilder();
                using (StringWriter writer = new StringWriter(sb))
                {
                    formattedNode.WriteTo(writer);
                    var r = writer.ToString();
                    return r;
                }
            }
        }
    }
}
