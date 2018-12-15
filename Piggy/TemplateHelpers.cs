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
        public static string GetFunctionReturn(string clang_reported_type)
        {
            // Clang ASTs have weird function values for Type. Extract out the
            // return type here.
            Regex regex = new Regex("(?<ret>[^(]*)[(].*[)].*");
            Match matches = regex.Match(clang_reported_type);
            string res = matches.Groups["ret"].Value;
            return res;
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
