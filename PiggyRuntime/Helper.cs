namespace PiggyRuntime
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;

    public class TemplateHelpers
    {
        public static string GetFunctionReturn(string clang_reported_type)
        {
            // Clang ASTs have weird function values for Type. Extract out the
            // return type here.
            Regex regex = new Regex("(?<ret>[^(]*)[(].*[)].*");
            Match matches = regex.Match(clang_reported_type);
            string res = matches.Groups["ret"].Value;
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
            var pointers = type.Split('*');
            if (pointers.Length == 2)
            {
                var bs = pointers[0].Trim();
                _type_map.TryGetValue(bs, out string result);
                if (result == null) return "out " + bs;
                return "out " + result;
            }
            else if (pointers.Length == 1)
            {
                var bs = pointers[0].Trim();
                _type_map.TryGetValue(bs, out string result);
                if (result == null) return pointers[0];
                return result;
            }
            else
            {
                var bs = pointers[0].Trim();
                _type_map.TryGetValue(bs, out string result);
                if (result == null) return "out IntPtr";
                return result;
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

