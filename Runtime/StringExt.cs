using System;
using System.Collections.Generic;
using System.Text;

namespace Runtime
{
    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string provide_escapes(this string s)
        {
            StringBuilder new_s = new StringBuilder();
            for (var i = 0; i != s.Length; ++i)
            {
                if (s[i] == '"' || s[i] == '\\')
                {
                    new_s.Append('\\');
                }
                new_s.Append(s[i]);
            }
            return new_s.ToString();
        }
    }
}
