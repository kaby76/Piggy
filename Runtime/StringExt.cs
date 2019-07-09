using System;
using System.CodeDom;
using System.IO;
using System.Text;
using System.CodeDom.Compiler;

namespace Runtime
{
    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
        private static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, new CodeGeneratorOptions { IndentString = "\t" });
                    var literal = writer.ToString();
                    literal = literal.Replace(string.Format("\" +{0}\t\"", Environment.NewLine), "");
                    return literal;
                }
            }
        }

        public static string provide_escapes(this string s)
        {
            StringBuilder new_s = new StringBuilder();
            new_s.Append(ToLiteral(s));
            //for (var i = 0; i != s.Length; ++i)
            //{
            //    if (s[i] == '"' || s[i] == '\\')
            //    {
            //        new_s.Append('\\');
            //    }
            //    new_s.Append(s[i]);
            //}
            return new_s.ToString();
        }
    }
}
