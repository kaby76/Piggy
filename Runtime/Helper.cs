namespace Runtime
{
    using System;
    using System.Text;

    public static class StringBuilderHelper
    {
        public static void AppendLine(this StringBuilder sb, string str)
        {
            sb.Append(str + Environment.NewLine);
        }
    }
}

