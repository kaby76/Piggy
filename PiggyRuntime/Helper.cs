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
            // Next, C# doesn't like declaring functions as "extern struct foobar fun()".
            // So, remove the struct/class designations in front.
            if (res.StartsWith("struct ")) res = res.Substring(7);
            else if (res.StartsWith("class ")) res = res.Substring(7);
            return res;
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

                // Apply some hacky surgery to get the type.
                // C# doesn't like declaring functions as "extern struct foobar fun()".
                // So, remove the struct/class designations in front.
                if (bs.StartsWith("struct ")) bs = bs.Substring(7);
                else if (bs.StartsWith("class ")) bs = bs.Substring(7);

                return "out " + bs;
            }
            else if (pointers.Length == 1)
            {
                var bs = pointers[0].Trim();
                _type_map.TryGetValue(bs, out string result);
                if (result != null) return result;

                // Apply some hacky surgery to get the type.
                // C# doesn't like declaring functions as "extern struct foobar fun()".
                // So, remove the struct/class designations in front.
                if (bs.StartsWith("struct ")) bs = bs.Substring(7);
                else if (bs.StartsWith("class ")) bs = bs.Substring(7);

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
            var pointers = type.Split('*');
            if (pointers.Length > 1)
            {
                // Pointer type.
                // Just make it IntPtr.
                return "IntPtr";
            }
            _type_map.TryGetValue(type, out string result);
            if (result == null) return type;
            return result;
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

