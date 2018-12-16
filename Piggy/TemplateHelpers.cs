using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Piggy
{
    public class TemplateHelpers
    {
        private Piggy _piggy;

        public TemplateHelpers(Piggy piggy)
        {
            _piggy = piggy;
        }

        public static string GetFunctionReturn(string clang_reported_type)
        {
            // Clang ASTs have weird function values for Type. Extract out the
            // return type here.
            Regex regex = new Regex("(?<ret>[^(]*)[(].*[)].*");
            Match matches = regex.Match(clang_reported_type);
            string res = matches.Groups["ret"].Value;
            return res;
        }

        public static string ModParamType(string type)
        {
            // Convert C++ types to C#.
            var c = type.Trim();
            if (c == "int") return "int";
            if (c == "uint") return "uint";
            if (c == "short") return "short";
            if (c == "ushort") return "ushort";
            if (c == "long long") return "long";
            if (c == "unsigned long long") return "ulong";
            if (c == "float") return "float";
            if (c == "double") return "double";
            if (c == "bool") return "bool";
            if (c == "char") return "int";
            return type;
        }

        public static bool BaseType(string type)
        {
            type = type.Trim();
            var b = type.Split(' ').ToList();
            if (b.Count > 1) return false;
            var c = b[0];
            if (c == "int") return false;
            if (c == "long") return false;
            if (c == "short") return false;
            if (c == "float") return false;
            if (c == "double") return false;
            if (c == "bool") return false;
            if (c == "char") return false;
            return true;
        }
    }

    public static class StringBuilderPlus
    {
        public static void AppendLine(this StringBuilder sb, string str)
        {
            sb.Append(str + Environment.NewLine);
        }
    }
}
