namespace PiggyRuntime
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

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

        public static string ModParamType(string type)
        {
            // Convert C++ types to C#.
            var c = type.Trim();
            if (c == "int") return "int";
            if (c == "uint") return "uint";
            if (c == "short") return "short";
            if (c == "ushort") return "ushort";
            if (c == "long") return "long";
            if (c == "unsigned long") return "ulong";
            if (c == "long long") return "long";
            if (c == "unsigned long long") return "ulong";
            if (c == "unsigned int") return "uint";
            if (c == "float") return "float";
            if (c == "double") return "double";
            if (c == "bool") return "bool";
            if (c == "char") return "int";
            return type;
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

