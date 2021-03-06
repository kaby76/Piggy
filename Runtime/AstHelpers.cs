﻿using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using gCSharp.CSharpSharwell;
using gJava;

namespace Runtime
{
    public class AstHelpers
    {
        private int changed = 0;
        private bool first_time = true;

        private void StartLine(StringBuilder sb, string file_name, IParseTree tree, CommonTokenStream stream, int level = 0)
        {
            if (changed - level >= 0)
            {
                if (!first_time)
                {
                    for (int j = 0; j < level; ++j) sb.Append("  ");
                    for (int k = 0; k < 1 + changed - level; ++k) sb.Append(") ");
                    sb.AppendLine();
                }
                changed = 0;
                first_time = false;
            }
            changed = level;
            for (int j = 0; j < level; ++j) sb.Append("  ");
        }
        public void ParenthesizedAST(StringBuilder sb, string file_name, IParseTree tree, CommonTokenStream stream, int level = 0)
        {
            // Antlr always names a non-terminal with first letter lowercase,
            // but renames it when creating the type in C#. So, remove the prefix,
            // lowercase the first letter, and remove the trailing "Context" part of
            // the name. Saves big time on output!
            if (tree as TerminalNodeImpl != null)
            {
                TerminalNodeImpl tok = tree as TerminalNodeImpl;
                Interval interval = tok.SourceInterval;
                var inter = stream.GetHiddenTokensToLeft(tok.Symbol.TokenIndex);
                if (inter != null)
                    foreach (var t in inter)
                    {
                        StartLine(sb, file_name, tree, stream, level);
                        sb.AppendLine("( HIDDEN text=" + t.Text.provide_escapes());
                    }
                StartLine(sb, file_name, tree, stream, level);
                sb.AppendLine("( TOKEN i=" + tree.SourceInterval.a 
                    + " t=" + tree.GetText().provide_escapes());
            }
            else
            {
                var fixed_name = tree.GetType().ToString()
                    .Replace("Antlr4.Runtime.Tree.", "");
                fixed_name = Regex.Replace(fixed_name, "^.*[+]", "");
                fixed_name = fixed_name.Substring(0, fixed_name.Length - "Context".Length);
                fixed_name = fixed_name[0].ToString().ToLower()
                             + fixed_name.Substring(1);
                StartLine(sb, file_name, tree, stream, level);
                sb.Append("( " + fixed_name);
                if (level == 0) sb.Append(" File=\""
                    + file_name
                    + "\"");
                sb.AppendLine();
            }
            for (int i = 0; i < tree.ChildCount; ++i)
            {
                var c = tree.GetChild(i);
                ParenthesizedAST(sb, file_name, c, stream, level + 1);
            }
            if (level == 0)
            {
                for (int k = 0; k < 1 + changed - level; ++k) sb.Append(") ");
                sb.AppendLine();
                changed = 0;
            }
        }

        public static void Reconstruct(IParseTree tree, CommonTokenStream stream)
        {
            if (tree as TerminalNodeImpl != null)
            {
                TerminalNodeImpl tok = tree as TerminalNodeImpl;
                Interval interval = tok.SourceInterval;
                var inter = stream.GetHiddenTokensToLeft(tok.Symbol.TokenIndex);
                if (inter != null)
                    foreach (var t in inter)
                    {
                        System.Console.Write(t.Text);
                    }
                var s = stream.GetText(interval);
                System.Console.Write(s);
            }
            else
            {
                for (int i = 0; i < tree.ChildCount; ++i)
                {
                    var c = tree.GetChild(i);
                    Reconstruct(c, stream);
                }
            }
        }

        private static CommonTokenStream tokens;
        public static void OpenTokenStream(string file_name)
        {
            var code_as_string = System.IO.File.ReadAllText(file_name);
            var input = new AntlrInputStream(code_as_string);
            var suffix = System.IO.Path.GetExtension(file_name);
            if (suffix == ".cs")
            {
                var lexer = new CSharpLexer(input);
                tokens = new CommonTokenStream(lexer);
            }
            else if (suffix == ".java")
            {
                var lexer = new JavaLexer(input);
                tokens = new CommonTokenStream(lexer);
            }
            else throw new System.Exception("File type not handled '" + suffix + "'");
            tokens.Fill();
        }

        public static string GetLeftOfToken(int index)
        {
            StringBuilder sb = new StringBuilder();
            var inter = tokens.GetHiddenTokensToLeft(index);
            if (inter != null)
                foreach (var t in inter)
                {
                    sb.Append(t.Text);
                }
            return sb.ToString();
        }
    }
}
