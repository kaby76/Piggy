template ClangSupport
{
    header {{

        public static string dllname = "need_to_set"; // Name of dll to load.
        public static string namespace_name = "Just_a_Default_Name"; // Namespace of generated code.
        public static string generate_for_only = ".*"; // default to every function, enum, struct, etc.
        public static string limit = ".*"; // default to every file.
        public static string output_location = PiggyRuntime.Tool.OutputLocation;

        // This is pretty much a hack to remove from Clang type strings the type of the function return.
        public static string GetFunctionReturn(string clang_reported_type)
        {
            // Clang ASTs have weird function values for Type. Extract out the
            // return type here.
            Regex regex = new Regex("(?<ret>[^(]*)[(].*[)].*");
            Match matches = regex.Match(clang_reported_type);
            string res = matches.Groups["ret"].Value;
            // Make sure it's trimmed.
            res = res.Trim();
            var bs = res;
            // Next, C# doesn't like declaring functions as "extern struct foobar fun()".
            // So, remove the struct/class designations in front.
            for (; ; )
            {
                if (bs.StartsWith("struct ")) bs = bs.Substring(7);
                else if (bs.StartsWith("class ")) bs = bs.Substring(6);
                else if (bs.StartsWith("union ")) bs = bs.Substring(6);
                else if (bs.StartsWith("const ")) bs = bs.Substring(6);
                break;
            }
            return bs;
        }

        public static Dictionary<string, string> _parm_type_map =
            new Dictionary<string, string>() {
            { "const char **", "out IntPtr" },
            { "char *", "[Out] byte[]"},
            { "unsigned int *", "out uint" },
            { "void **", "out IntPtr" },
            { "void *", "IntPtr" },
            { "const char *", "string" },
            { "const void *", "IntPtr" },
            { "const <type> *", "in <type>"},
        };
        
        // These types are used to map return values from functions.
        // These are not used for parameter types to functions.
        // So, no "out", no "in", no "ref". Note also that C# kind of sucks
        // in syntax here for marshalling. You cannot return a "string" unless
        // you have a [return ...] attribute. I didn't invent C#'s syntax.
        // For now, let's just return IntPtr for things like that.
        // As far as I can tell, just return value types, no reference types--except as IntPtr.
        // See https://limbioliong.wordpress.com/2011/06/16/returning-strings-from-a-c-api/
        public static Dictionary<string, string> _type_map =
            new Dictionary<string, string>() {
            { "size_t", "SizeT" },
            { "int", "int"},
            { "uint", "uint"},
            { "short", "short"},
            { "ushort", "ushort"},
            { "long", "long"},
            { "unsigned char", "byte" },
            { "unsigned short", "UInt16"},
            { "unsigned int", "uint"},
            { "unsigned long", "ulong"},
            { "unsigned long long", "ulong"},
            { "long long", "long"},
            { "float", "float"},
            { "double", "double"},
            { "bool", "bool"},
            { "char", "byte"},
            { "const char *", "IntPtr" }, // For now, don't do [return: MarshalAs(UnmanagedType.LPStr)] set up of function.
            { "char *", "IntPtr" },
            { "signed char", "sbyte" },
        };

        public static string RewriteAppliedOccurrence(bool is_param, string type)
        {
            // Note, this routine should be following the recommendations in
            // https://docs.microsoft.com/en-us/dotnet/framework/interop/passing-structures
            type = type.Trim();
            type = type.Split(':')[0];
            string r;
            if (is_param)
            {
                _parm_type_map.TryGetValue(type, out string r2);
                r = r2;
            }
            else
            {
                _type_map.TryGetValue(type, out string r3);
                r = r3;
            }
            if (r != null) return r;
            string[] pointers = type.Split('*');
            if (is_param && pointers.Length == 2)
            {
                var bs = pointers[0].Trim();

                // Apply some hacky surgery to get the type.
                // C# doesn't like declaring functions as "extern struct foobar fun()".
                // So, remove the struct/class designations in front.
                for (; ; )
                {
                    if (bs.StartsWith("struct ")) bs = bs.Substring(7);
                    else if (bs.StartsWith("class ")) bs = bs.Substring(6);
                    else if (bs.StartsWith("union ")) bs = bs.Substring(6);
                    else if (bs.StartsWith("const ")) bs = bs.Substring(6);
                    break;
                }

                string result;
                if (is_param)
                {
                    _parm_type_map.TryGetValue(bs, out string r2);
                    result = r2;
                }
                else
                {
                    _type_map.TryGetValue(bs, out string r3);
                    result = r3;
                }
                if (result != null) return "ref " + result;

                return "ref " + bs;
            }
            else if (pointers.Length == 1)
            {
                var bs = pointers[0].Trim();

                // Apply some hacky surgery to get the type.
                // C# doesn't like declaring functions as "extern struct foobar fun()".
                // So, remove the struct/class designations in front.
                for (;;)
                {
                    if (bs.StartsWith("struct ")) bs = bs.Substring(7);
                    else if (bs.StartsWith("class ")) bs = bs.Substring(6);
                    else if (bs.StartsWith("union ")) bs = bs.Substring(6);
                    else if (bs.StartsWith("const ")) bs = bs.Substring(6);
                    break;
                }

                string result;
                if (is_param)
                {
                    _parm_type_map.TryGetValue(bs, out string r2);
                    result = r2;
                }
                else
                {
                    _type_map.TryGetValue(bs, out string r3);
                    result = r3;
                }
                if (result != null) return result;

                return bs;
            }
            else
            {
                // Here we assume two levels of indirection is meant to be a in/out pointer.
                if (is_param)
                    return "ref IntPtr";
                else
                    return "IntPtr";
            }
        }

        public static void AddAppliedOccurrenceRewrites(bool is_param, Dictionary<string, string> additions)
        {
            foreach (var kvp in additions)
            {
                var type = kvp.Key;
                var rewrite = kvp.Value;
                type = type.Trim();
                type = type.Split(':')[0];
                if (is_param)
                    _parm_type_map[type] = rewrite;
                else
                    _type_map[type] = rewrite;
            }
        }


        public static bool IsAppliedOccurrenceRewrite(bool is_param, string type)
        {
            type = type.Trim();
            type = type.Split(':')[0];
            if (is_param)
                return _parm_type_map.ContainsKey(type) || _type_map.ContainsValue(type);
            else
                return _type_map.ContainsKey(type) || _type_map.ContainsValue(type);
        }

        public static bool IsAppliedOccurrencePointerType(string type)
        {
            type = type.Trim();
            type = type.Split(':')[0];
            string[] pointers = type.Split('*');
            return pointers.Length > 1;
        }

        public static Dictionary<string, string> _name_map =
            new Dictionary<string, string>() {
            { "char", "@char" },
            { "int", "@int"},
            { "uint", "@uint" },
            { "void", "@void" },
            { "string", "@string" },
        };

        public static string EscapeCsharpNames(string pre_name)
        {
            if (_name_map.TryGetValue(pre_name, out string result))
                return result;
            return pre_name;
        }

        public static void FormatFile(string file_name)
        {
            string sa = System.IO.File.ReadAllText(file_name);
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
                System.IO.File.WriteAllText(file_name, r);
            }
        }
    }}

    pass Start {
        ( TranslationUnitDecl )
    }

    pass End {
        ( TranslationUnitDecl )
    }
}
