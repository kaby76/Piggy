template ClangSupport
{
    header {{

        public static string dllname = "need_to_set"; // Name of dll to load.
        public static string namespace_name = "Just_a_Default_Name"; // Namespace of generated code.
        public static string generate_for_only = ".*"; // default to every function, enum, struct, etc.
		public static string limit = ".*"; // default to every file.

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

        public static string ModParamUsageType(string type)
        {
            type = type.Trim();
            type = type.Split(':')[0];
            _parm_type_map.TryGetValue(type, out string r);
            if (r != null) return r;
            string[] pointers = type.Split('*');
            if (pointers.Length == 2)
            {
                var bs = pointers[0].Trim();
                _type_map.TryGetValue(bs, out string result);
                if (result != null) return "out " + result;

                string use_out = "out ";
                if (bs.StartsWith("const "))
                {
                    bs = bs.Substring(6);
                    use_out = "";
                }

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

                return use_out + bs;
            }
            else if (pointers.Length == 1)
            {
                var bs = pointers[0].Trim();
                _type_map.TryGetValue(bs, out string result);
                if (result != null) return result;

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

                return bs;
            }
            else
            {
                var bs = pointers[0].Trim();
                _type_map.TryGetValue(bs, out string result);
                if (result != null) return result;

                return "out IntPtr";
            }
        }

        public static string ModParamUsageType(Dictionary<string, string> additions)
        {
            foreach (var kvp in additions)
            {
                var type = kvp.Key;
                var rewrite = kvp.Value;
                type = type.Trim();
                type = type.Split(':')[0];
                _parm_type_map[type] = rewrite;
            }
            return null;
        }

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

        public static string ModNonParamUsageType(string type)
        {
            type = type.Trim();
            type = type.Split(':')[0];
            _type_map.TryGetValue(type, out string r);
            if (r != null) return r;

            string[] pointers = type.Split('*');
            if (pointers.Length > 1)
            {
                // Pointer type.
                // Just make it IntPtr.
                return "IntPtr";
            }
            else
            {
                _type_map.TryGetValue(type, out string result);
                if (result != null) return result;

                var bs = type;
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

                return bs;
            }
        }

        public static string ModNonParamUsageType(Dictionary<string, string> additions)
        {
            foreach (var kvp in additions)
            {
                var type = kvp.Key;
                var rewrite = kvp.Value;
                type = type.Trim();
                type = type.Split(':')[0];
                _type_map[type] = rewrite;
            }
            return null;
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
    }}
}
