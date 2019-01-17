namespace PiggyRuntime
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;

    public class TemplateHelpers
    {
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

        private static Dictionary<string, string> _parm_type_map = new Dictionary<string, string>()
        {
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

        private static Dictionary<string, string> _type_map = new Dictionary<string, string>()
        {
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
    }

    public static class StringBuilderHelper
    {
        public static void AppendLine(this StringBuilder sb, string str)
        {
            sb.Append(str + Environment.NewLine);
        }
    }
}

