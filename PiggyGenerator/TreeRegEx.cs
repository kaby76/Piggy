namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;
    using Antlr4.Runtime;
    using PiggyRuntime;
    using System.Collections.Generic;
    using System;
    using System.Text.RegularExpressions;

    public class TreeRegEx
    {
        public IParseTree _ast;
        public Type _current_type;
        public object _instance;
        public CommonTokenStream _common_token_stream;
        public HashSet<IParseTree> _matches = new HashSet<IParseTree>();
        public HashSet<IParseTree> _top_level_matches = new HashSet<IParseTree>();
        public Intercept<IParseTree, Path> _matches_path_start = new Intercept<IParseTree, Path>();
        public static Dictionary<IParseTree, IParseTree> _parent = new Dictionary<IParseTree, IParseTree>();
        public static Dictionary<string, IParseTree> tree_collections = new Dictionary<string, IParseTree>();
        public List<Pass> _passes;
        public Piggy _piggy;

        public TreeRegEx(Piggy piggy, List<Pass> passes_with_common_name, object instance)
        {
            _piggy = piggy;
            _ast = _piggy._ast.GetChild(0);
            _passes = passes_with_common_name;
            _instance = instance;
            _common_token_stream = _piggy._common_token_stream;
        }

        public void Match()
        {
            foreach (var pass in this._passes)
            {
                _current_type = pass.Owner.Type;
                // Combine all patterns of a pass into one NFA,
                // then one, beautifully massive DFA.
                var nfa = new Automaton();
                foreach (Pattern pattern in pass.Patterns) NFA.post2nfa(nfa, pattern);
                System.Console.Error.WriteLine(nfa);
                var nfa_to_dfa = new NFAToDFA();
                var dfa = nfa_to_dfa.ConvertToDFA(nfa);
                System.Console.Error.WriteLine(dfa);

                // Perform naive matching for each node.
                foreach (var ast_node in this._ast.Preorder())
                {
                    var t = ast_node.GetText();
                    if (ast_node.GetText().Contains("EnumDecl"))
                    { }

                    List<Path> MatchingPaths = new List<Path>();
                    var nfa_match = new NfaMatch(this._ast.Parents(),
                        this._piggy._code_blocks, this._instance);
                    bool has_previous_match = _matches.Contains(ast_node);
                    bool do_matching = (!has_previous_match);
                    var matched = do_matching && nfa_match.FindMatches(MatchingPaths, dfa, ast_node);
                    if (matched)
                    {
                        // If this node matched, then mark entire subtree as matched.
                        var stack = new Stack<IParseTree>();
                        stack.Push(ast_node);
                        while (stack.Count > 0)
                        {
                            var v = stack.Pop();
                            _matches.Add(v);
                            for (int i = v.ChildCount - 1; i >= 0; --i)
                            {
                                var c = v.GetChild(i);
                                stack.Push(c);
                            }
                        }
                        _top_level_matches.Add(ast_node);
                        foreach (Path p in MatchingPaths)
                        {
                            _matches_path_start.MyAdd(ast_node, p);
                        }
                    }
                }
            }
        }

        private bool IsPatternSimple(IParseTree p)
        {
            var q = p.GetChild(0);
            return q as SpecParserParser.Simple_basicContext != null;
        }

        private bool IsPatternKleene(IParseTree p)
        {
            var q = p.GetChild(0);
            return q as SpecParserParser.Kleene_star_basicContext != null;
        }

        public static string GetText(IParseTree context)
        {
            if (context as Antlr4.Runtime.Tree.TerminalNodeImpl != null)
            {
                return context.GetText();
            }
            var x = context as Antlr4.Runtime.ParserRuleContext;
            if (x == null)
            {
                return "UNKNOWN TYPE!";
            }
            var c = x;
            IToken startToken = c.Start;
            IToken stopToken = c.Stop;
            ICharStream cs = startToken.InputStream;
            int startIndex = startToken.StartIndex;
            int stopIndex = stopToken.StopIndex;
            if (startIndex > stopIndex)
                startIndex = stopIndex;
            return cs.GetText(new Antlr4.Runtime.Misc.Interval(startIndex, stopIndex));
        }

        #region MyRegion

        public bool is_pattern_not_attr(IParseTree p)
        {
            if (p == null) return false;
            if (p as SpecParserParser.MoreContext == null) return false;
            p = p.GetChild(0);
            SpecParserParser.AttrContext attr = p as SpecParserParser.AttrContext;
            if (attr == null) return false;
            return attr.GetText().StartsWith("!");
        }


        public bool is_pattern_attr(IParseTree p)
        {
            if (p == null) return false;
            if (p as SpecParserParser.MoreContext == null) return false;
            p = p.GetChild(0);
            SpecParserParser.AttrContext attr = p as SpecParserParser.AttrContext;
            return attr != null;
        }

        public static bool is_pattern_star(IParseTree p)
        {
            if (p == null) return false;
            if (p as SpecParserParser.MoreContext == null) return false;
            p = p.GetChild(0);
            if (p as SpecParserParser.RexpContext == null) return false;
            var s = p.GetChild(0);
            for (int i = 0; i < s.ChildCount; i += 2)
            {
                var q = s as SpecParserParser.Simple_rexpContext;
                if (q == null) return false;
                var r = q.GetChild(0);
                var t = r as SpecParserParser.Basic_rexpContext;
                if (t == null) return false;
                var v = t.GetChild(0);
                if (v as SpecParserParser.Star_rexpContext != null) return true;
            }
            return false;
        }

        public static bool is_pattern_plus(IParseTree p)
        {
            if (p == null) return false;
            if (p as SpecParserParser.MoreContext == null) return false;
            p = p.GetChild(0);
            if (p as SpecParserParser.RexpContext == null) return false;
            var s = p.GetChild(0);
            for (int i = 0; i < s.ChildCount; i += 2)
            {
                var q = s as SpecParserParser.Simple_rexpContext;
                if (q == null) return false;
                var r = q.GetChild(0);
                var t = r as SpecParserParser.Basic_rexpContext;
                if (t == null) return false;
                var v = t.GetChild(0);
                if (v as SpecParserParser.Plus_rexpContext != null) return true;
            }
            return false;
        }


        public bool is_ast_attr(IParseTree p)
        {
            if (p == null) return false;
            AstParserParser.AttrContext attr = p as AstParserParser.AttrContext;
            return attr != null;
        }

        public bool is_text(IParseTree p)
        {
            var p_child = p.GetChild(0);
            SpecParserParser.TextContext text =
                p_child as SpecParserParser.TextContext;
            return text != null;
        }

        private bool is_pattern_simple(IParseTree p)
        {
            var q = p.GetChild(0);
            return q as SpecParserParser.Simple_basicContext != null;
        }

        private bool is_pattern_kleene(IParseTree p)
        {
            var q = p.GetChild(0);
            return q as SpecParserParser.Kleene_star_basicContext != null;
        }

        private bool is_pattern_continued(IParseTree p)
        {
            var q = p.GetChild(0);
            return q as SpecParserParser.Continued_basicContext != null;
        }

        string ReplaceMacro(IParseTree p)
        {
            // Try in order current type, then all other types.
            try
            {
                var main = _piggy._code_blocks[p];
                Type current_type = _current_type;
                object instance = this._instance;
                object[] a = new object[] { };
                var res = main.Invoke(instance, a);
                return res as string;
            }
            catch (Exception e)
            {
            }
            throw new Exception("Cannot eval expression.");
        }

        private bool match_attr(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.AttrContext p_attr = p as SpecParserParser.AttrContext;
            if (p_attr == null) return false;
            AstParserParser.AttrContext t_attr = t as AstParserParser.AttrContext;
            if (t_attr == null) return false;
            int pos = 0;
            var p_id = p_attr.GetChild(pos);
            var t_id = t_attr.GetChild(pos);
            if (p_id.GetText() == "!")
            {
                pos++;
                p_id = p_attr.GetChild(pos);
                if (p_id.GetText() == t_id.GetText()) return true;
                else return false;
            }
            else
            {
                if (p_id.GetText() != t_id.GetText()) return false;
            }
            pos++;
            pos++;
            var p_val = p_attr.GetChild(pos);
            var t_val = t_attr.GetChild(pos);
            string pattern = p_val.GetText();
            if (pattern == "*")
            {
                return true;
            }

            if (pattern.StartsWith("$\""))
            {
                pattern = pattern.Substring(2);
                pattern = pattern.Substring(0, pattern.Length - 1);
                try
                {
                    pattern = ReplaceMacro(p_val);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Cannot perform substitution in pattern with string.");
                    System.Console.WriteLine("Pattern " + pattern);
                    System.Console.WriteLine(e.Message);
                    throw e;
                }
                pattern = pattern.Replace("\\", "\\\\");
            }
            else
            {
                pattern = pattern.Substring(1);
                pattern = pattern.Substring(0, pattern.Length - 1);
            }

            Regex re = new Regex(pattern);
            string tvaltext = t_val.GetText();
            tvaltext = tvaltext.Substring(1);
            tvaltext = tvaltext.Substring(0, tvaltext.Length - 1);
            var matched = re.Match(tvaltext);
            var result = matched.Success;
            return result;
        }
        #endregion
    }
}
