﻿namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;
    using Antlr4.Runtime;
    using PiggyRuntime;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System;

    public class TreeRegEx
    {
        public IParseTree _ast;
        public Type _current_type;
        public object _instance;
        public CommonTokenStream _common_token_stream;
        public Intercept<IParseTree, IParseTree> _matches = new Intercept<IParseTree, IParseTree>();
        public Intercept<IParseTree, IParseTree> _top_level_matches = new Intercept<IParseTree, IParseTree>();
        public Dictionary<IParseTree, IParseTree> _parent = new Dictionary<IParseTree, IParseTree>();
        public List<Pass> _passes;
        public Piggy _piggy;
        public List<IParseTree> _post_order = new List<IParseTree>();
        public List<IParseTree> _pre_order = new List<IParseTree>();
        public Dictionary<IParseTree, int> _pre_order_number = new Dictionary<IParseTree, int>();

        public TreeRegEx(Piggy piggy, List<Pass> passes_with_common_name, object instance)
        {
            _piggy = piggy;
            _ast = _piggy._ast.GetChild(0);
            _passes = passes_with_common_name;
            _instance = instance;
            _common_token_stream = _piggy._common_token_stream;

            bool result = false;
            var visited = new HashSet<IParseTree>();
            var stack = new Stack<IParseTree>();
            _parent = Parents.Compute(_ast);
            foreach (var pass in _passes)
            {
                foreach (var pattern in pass.Patterns)
                {
                    stack.Push(pattern.AstNode);
                    while (stack.Count > 0)
                    {
                        var v = stack.Pop();
                        if (visited.Contains(v))
                            continue;
                        visited.Add(v);
                        for (int i = v.ChildCount - 1; i >= 0; --i)
                        {
                            var c = v.GetChild(i);
                            _parent[c] = v;
                            if (!visited.Contains(c))
                                stack.Push(c);
                        }
                    }
                }
            }

            _pre_order = new List<IParseTree>();
            int current_dfs_number = 0;
            visited = new HashSet<IParseTree>();
            stack.Push(_ast);
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (visited.Contains(v)) continue;
                visited.Add(v);
                _pre_order_number[v] = current_dfs_number;
                _pre_order.Add(v);
                for (int i = v.ChildCount - 1; i >= 0; --i)
                {
                    var c = v.GetChild(i);
                    if (!visited.Contains(c))
                        stack.Push(c);
                }
            }

            var copy = new Stack<IParseTree>(_pre_order);
            _post_order = new List<IParseTree>();
            while (copy.Any())
            {
                var x = copy.Pop();
                _post_order.Add(x);
            }
        }

        // Pattern matcher.
        public static string sourceTextForContext(IParseTree context)
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

        public void dfs_match()
        {
            var visited = new HashSet<IParseTree>();
            var stack = new Stack<IParseTree>();
            stack.Push(_ast);

            foreach (var v in _pre_order)
            {
                foreach (var pass in this._passes)
                {
                    foreach (var pattern in pass.Patterns)
                    {
                        SpecParserParser.PatternContext t = pattern.AstNode as SpecParserParser.PatternContext;
                        _current_type = pattern.Owner.Owner.Type;
                        // Try matching at vertex, if the node hasn't been already matched.
                        _matches.TryGetValue(v, out List<IParseTree> val);
                        bool do_matching = val == null || !val.Where(xx => is_pattern_kleene(xx) || is_pattern_simple(xx)).Any();
                        if (do_matching)
                        {
                            //System.Console.WriteLine("Trying match ");
                            //System.Console.WriteLine("Template " + sourceTextForContext(t));
                            //System.Console.WriteLine("Tree " + sourceTextForContext(v));
                            bool matched = match_pattern(t, v);
                            if (matched)
                            {
                                var tre = new Tree(_parent, _ast, v, _common_token_stream);
                                match_pattern(t, v, true);
                            }
                        }
                    }
                }
            }
        }

        /*
         * Recursively go down _display_ast and search for a match anywhere of the pattern tree.
         *
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_kleene_star_node(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.Kleene_star_basicContext pstar = p as SpecParserParser.Kleene_star_basicContext;
            if (pstar == null) return false;
            AstParserParser.DeclContext t_decl = t as AstParserParser.DeclContext;
            if (t_decl == null) return false;

            // Go down tree node, looking for match in _display_ast.
            var stack = new Stack<IParseTree>();
            stack.Push(t_decl);
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                var r = match_basic_simple(pstar, v, map);
                if (r)
                {
                    if (map) _matches.MyAdd(t, p);
                    return true;
                }
                for (int i = v.ChildCount - 1; i >= 0; --i)
                {
                    var c = v.GetChild(i);
                    stack.Push(c);
                }
            }
            return false;
        }

        /*
         * Match pattern: basic ;
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_pattern(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.PatternContext pattern = p as SpecParserParser.PatternContext;
            if (pattern == null) return false;
            var re = p.GetChild(0);
            if (re == null) return false;
            bool result = match_basic(re, t, map);
            if (result && map)
            {
                _matches.MyAdd(t, p);
                _top_level_matches.MyAdd(t, p);
            }
            return result;
        }

        /*
         * Match rexp : simple_rexp (OR simple_rexp)* ;
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_rexp(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.RexpContext re = p as SpecParserParser.RexpContext;
            if (re == null) return false;
            int pos = 0;
            IParseTree start = re.GetChild(0);
            bool result = match_simple_re(start, t, map);
            if (result && map) _matches.MyAdd(t, p);
            if (result) return true;
            for (; ; )
            {
                pos += 2;
                start = re.GetChild(pos);
                if (start == null) break;
                result = match_simple_re(start, t, map);
                if (result && map) _matches.MyAdd(t, p);
                if (result) return true;
            }
            return false;
        }

        /* Match simple_rexp : basic_rexp ;
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_simple_re(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.Simple_rexpContext simple_re =
                p as SpecParserParser.Simple_rexpContext;
            if (simple_re == null) return false;
            int pos = 0;
            IParseTree start = simple_re.GetChild(0);
            var result = match_basic_re(start, t, map);
            if (!result) return false;
            if (map) _matches.MyAdd(t, p);
            return true;
        }

        /* Match basic_rexp : star_rexp | plus_rexp | elementary_rexp ;
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_basic_re(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.Basic_rexpContext basic_re =
                p as SpecParserParser.Basic_rexpContext;
            if (basic_re == null) return false;
            var child = basic_re.GetChild(0);
            if (child == null) return false;

            SpecParserParser.Star_rexpContext star_rexp =
                child as SpecParserParser.Star_rexpContext;
            if (star_rexp != null)
            {
                var result = match_star_rexp(star_rexp, t, map);
                if (result && map) _matches.MyAdd(t, p);
                return result;
            }
            SpecParserParser.Plus_rexpContext plus_rexp =
                child as SpecParserParser.Plus_rexpContext;
            if (plus_rexp != null)
            {
                var result = match_plus_rexp(plus_rexp, t, map);
                if (result && map) _matches.MyAdd(t, p);
                return result;
            }
            {
                var result = match_elementary_rexp(child, t, map);
                if (result && map) _matches.MyAdd(t, p);
                return result;
            }
        }

        /*
         * star_rexp: elementary_rexp STAR;
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_star_rexp(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.Star_rexpContext star_rexp =
                p as SpecParserParser.Star_rexpContext;
            if (star_rexp == null) return false;
            int pos = 0;
            var child = star_rexp.GetChild(0);
            if (child == null)
            {
                // It is possible that there are no children for AST.
                // But, this is OK because it still matches.
                if (map) _matches.MyAdd(t, p);
                return true;
            }
            // Match zero or more of elementary. Note, we are matching
            // a elementary_rexp with a "more" type in the _display_ast.
            bool result = match_elementary_rexp(child, t, map);
            if (result && map) _matches.MyAdd(t, p);
            return result;
        }

        /*
         * plus_rexp: elementary_rexp PLUS;
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_plus_rexp(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.Plus_rexpContext plus_rexp =
                p as SpecParserParser.Plus_rexpContext;
            if (plus_rexp == null) return false;
            var child = plus_rexp.GetChild(0);
            if (child == null) return false;
            bool result = match_elementary_rexp(child, t, map);
            if (result && map) _matches.MyAdd(t, p);
            return result;
        }

        /*
         * elementary_rexp: group_rexp | basic ;
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_elementary_rexp(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.Elementary_rexpContext elementary_rexp =
                p as SpecParserParser.Elementary_rexpContext;
            if (elementary_rexp == null) return false;
            var child = elementary_rexp.GetChild(0);
            if (child == null) return false;
            SpecParserParser.Group_rexpContext group_rexp =
                child as SpecParserParser.Group_rexpContext;
            if (group_rexp != null)
            {
                var result = match_group_rexp(group_rexp, t, map);
                if (result && map) _matches.MyAdd(t, p);
                return result;
            }
            SpecParserParser.BasicContext basic = child as SpecParserParser.BasicContext;
            if (basic != null)
            {
                var result = match_basic(basic, t, map);
                if (result && map) _matches.MyAdd(t, p);
                return result;
            }
            return false;
        }

        /*
         * pattern grammar--
         * group_rexp:   OPEN_RE rexp CLOSE_RE ;
         *
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_group_rexp(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.Group_rexpContext group_rexp = p as SpecParserParser.Group_rexpContext;
            if (group_rexp == null) return false;
            var child = group_rexp.GetChild(1);
            if (child == null) return false;
            var result = match_rexp(child, t, map);
            if (result && map) _matches.MyAdd(t, p);
            return result;
        }

        public bool is_pattern_not_attr(IParseTree p)
        {
            if (p == null) return false;
            if (p as SpecParserParser.MoreContext == null) return false;
            p = p.GetChild(0);
            SpecParserParser.AttrContext attr = p as SpecParserParser.AttrContext;
            if (attr == null) return false;
            return attr.GetText().StartsWith("!");
        }


        /*
         * Determine via lookahead if a node for a pattern matcher
         * is an attribute or not.
         */
        public bool is_pattern_attr(IParseTree p)
        {
            if (p == null) return false;
            if (p as SpecParserParser.MoreContext == null) return false;
            p = p.GetChild(0);
            SpecParserParser.AttrContext attr = p as SpecParserParser.AttrContext;
            return attr != null;
        }

        /*
         * Determine via lookahead if a node for a pattern matcher
         * is a star expression or not.
         */
        public bool is_pattern_star(IParseTree p)
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

        /*
         * Determine via lookahead if a node for a pattern matcher
         * is a plus expression or not.
         */
        public bool is_pattern_plus(IParseTree p)
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


        /*
         * Determine via lookahead if a node for a pattern matcher
         * is an attribute or not.
         */
        public bool is_ast_attr(IParseTree p)
        {
            if (p == null) return false;
            AstParserParser.AttrContext attr = p as AstParserParser.AttrContext;
            return attr != null;
        }

        /*
         * Determine via lookahead if a node for a pattern matcher
         * is text or not.
         */
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

        private bool match_basic(IParseTree p, IParseTree t, bool map = false)
        {
            var q = p.GetChild(0);
            bool result = false;
            if (q as SpecParserParser.Simple_basicContext != null)
            {
                result = match_basic_simple(q, t, map);
            }
            else if (q as SpecParserParser.Kleene_star_basicContext != null)
            {
                result = match_kleene_star_node(q, t, map);
            }
            else if (q as SpecParserParser.Continued_basicContext != null)
            {
                result = match_basic_simple(q, t, map);
            }
            if (result && map) _matches.MyAdd(t, p);
            return result;
        }

        /*
         * pattern grammar--
         * basic_simple: OPEN_PAREN id_or_star_or_empty more* CLOSE_PAREN ;
         *
         * tree grammar--
         * decl : OPEN_PAREN ID more* CLOSE_PAREN ;
         *
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_basic_simple(IParseTree p, IParseTree t, bool map = false)
        {
            // Match open paren.
            int t_pos = 0;
            int p_pos = 0;
            var decl = t as AstParserParser.DeclContext;
            if (decl == null) return false;

            {
                var t_c = decl.GetChild(t_pos);
                var t_tok = t_c as TerminalNodeImpl;
                if (t_tok == null) return false;
                var t_sym = t_tok.Symbol;
                if (t_sym.Type != AstLexer.OPEN_PAREN) return false;
                var p_c = p.GetChild(p_pos);
                var p_tok = p_c as TerminalNodeImpl;
                if (p_tok == null) return false;
                var p_sym = p_tok.Symbol;
                if (!(p_sym.Type == SpecParserParser.OPEN_PAREN ||
                    p_sym.Type == SpecParserParser.OPEN_KLEENE_STAR_PAREN ||
                    p_sym.Type == SpecParserParser.OPEN_VISIT))
                    return false;

                if (map) _matches.MyAdd(t_c, p_c);
            }

            p_pos++;
            t_pos++;

            // Match ID, if supplied.
            var id_tree = decl.GetChild(t_pos);
            var id_or_star = p.GetChild(p_pos);
            if (id_or_star as SpecParserParser.Id_or_star_or_emptyContext == null)
                return false;
            var id = id_or_star.GetChild(0);

            if (id == null)
            {
                p_pos++;
                t_pos++;
            }
            else if (id.GetText() == "*")
            {
                if (map) _matches.MyAdd(id_tree, id);
                p_pos++;
                t_pos++;
            }
            else if (id.GetText() != id_tree.GetText())
                return false;
            else
            {
                if (map) _matches.MyAdd(id_tree, id);
                t_pos++;
                p_pos++;
            }
            if (map)
            {
                _matches.MyAdd(id_tree, id_or_star);
            }

            // We are now at "more" in both t and p.
            // p contains list of "more", as well as t.
            // p's items should all match t's in order.

            int p_pos_max = p.ChildCount;
            int t_pos_max = t.ChildCount;
            IParseTree p_more = null;
            IParseTree t_more = null;
            bool result = true;
            int not_counting_parens = 1;

            for (; ; )
            {
                if (p_pos >= p.ChildCount - not_counting_parens)
                    break;
                if (t_pos >= decl.ChildCount - not_counting_parens)
                    break;

                // Fetch pattern "more", ignoring code and text in pattern.
                p_more = p.GetChild(p_pos);
                if (p_more == null) break;
                var p_more_type = p_more.GetType();
                var p_more_text = p_more.GetText();
                if (p_more as SpecParserParser.CodeContext != null || p_more as SpecParserParser.TextContext != null)
                {
                    p_pos++;
                    continue;
                }
                SpecParserParser.MoreContext c11 = p_more as SpecParserParser.MoreContext;
                if (c11 == null) return false;
                var p_child = p_more.GetChild(0);
                if (p_child as SpecParserParser.CodeContext != null || p_child as SpecParserParser.TextContext != null)
                {
                    p_pos++;
                    continue;
                }
                var p_child_type = p_child.GetType();

                // Fetch tree "more".
                t_more = decl.GetChild(t_pos);
                var t_more_type = t_more.GetType();
                var t_more_text = t_more.GetText();
                AstParserParser.MoreContext c22 = t_more as AstParserParser.MoreContext;
                if (c22 == null) return false;

                // Compare pattern at p_pos with tree at t_pos, ignoring code and text in pattern.

                // Note order of attributes is significant.
                // Go through _display_ast and look for pattern in the _display_ast. If we
                // found any match, then record this point in the _display_ast matched.
                // Continue matching until the end of the _display_ast, which ends in
                // a ")". Note, if the pattern is a "*" or "+" expression,
                // we keep looking for this pattern. Otherwise, we skip to the
                // next pattern child.
                bool is_not_attr = this.is_pattern_not_attr(p_more);
                bool is_attr = this.is_pattern_attr(p_more);
                bool is_plus = this.is_pattern_plus(p_more);
                bool is_star = this.is_pattern_star(p_more);

                bool matched = match_more(c11, c22, map);
                if (matched)
                {
                    if (is_not_attr)
                    {
                        // If you find an attribute with a !attr pattern, then this pattern can't match!
                        return false;
                    }
                    t_pos++;
                    if (!(is_plus || is_star))
                    {
                        p_pos++;
                    }
                    continue;
                }

                // mismatch...
                // In general, a mismatch in a "more" part of a pattern is not a problem.
                t_pos++;
            }

            // At the end of pattern match expression or the tree expression.
            // Step through pattern and make sure we are at the end, less code or text blocks.
            for (; ; )
            {
                if (p_pos >= p.ChildCount - not_counting_parens)
                    break;

                // Fetch pattern "more", ignoring code and text in pattern.
                p_more = p.GetChild(p_pos);
                if (p_more == null) break;
                var p_more_type = p_more.GetType();
                var p_more_text = p_more.GetText();
                if (p_more as SpecParserParser.CodeContext != null || p_more as SpecParserParser.TextContext != null)
                {
                    p_pos++;
                    continue;
                }
                SpecParserParser.MoreContext c11 = p_more as SpecParserParser.MoreContext;
                if (c11 == null) return false;
                var p_child = p_more.GetChild(0);
                if (p_child as SpecParserParser.CodeContext != null || p_child as SpecParserParser.TextContext != null)
                {
                    p_pos++;
                    continue;
                }

                bool is_not_attr = this.is_pattern_not_attr(p_more);
                bool is_attr = this.is_pattern_attr(p_more);
                bool is_plus = this.is_pattern_plus(p_more);
                bool is_star = this.is_pattern_star(p_more);

                // Assume if it's a plus or star that we can skip past it.
                if (is_plus || is_star)
                {
                    p_pos++;
                    continue;
                }

                return false;
            }

            {
                if (p_pos == p.ChildCount - not_counting_parens)
                {
                    t_pos = decl.ChildCount - 2;
                }
                var t_c = decl.GetChild(t_pos + 1);
                var t_tok = t_c as TerminalNodeImpl;
                if (t_tok == null)
                    return false;
                var t_sym = t_tok.Symbol;
                if (t_sym.Type != AstLexer.CLOSE_PAREN)
                    return false;
                var p_c = p.GetChild(p_pos);
                var p_tok = p_c as TerminalNodeImpl;
                if (p_tok == null)
                    return false;
                var p_sym = p_tok.Symbol;
                if (!(p_sym.Type == SpecParserParser.CLOSE_PAREN
                      || p_sym.Type == SpecParserParser.CLOSE_KLEENE_STAR_PAREN
                      || p_sym.Type == SpecParserParser.CLOSE_VISIT))
                    return false;
                if (map) _matches.MyAdd(t_c, p_c);
            }

            if (map) _matches.MyAdd(t, p);
            return true;
        }

        /*
         * pattern grammar--
         * more : rexp | text | code | attr ;
         *
         * tree grammar--
         * more : decl | attr ;
         *
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_more(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.MoreContext p_more = p as SpecParserParser.MoreContext;
            if (p_more == null) return false;
            AstParserParser.MoreContext t_more = t as AstParserParser.MoreContext;
            if (t_more == null) return false;
            bool result = true;
            var p_child = p_more.GetChild(0);
            if (p_child as SpecParserParser.CodeContext != null
                || p_child as SpecParserParser.TextContext != null)
            {
                if (map) _matches.MyAdd(t, p);
                return true;
            }
            var t_child = t.GetChild(0);
            SpecParserParser.RexpContext rexp = p_child as SpecParserParser.RexpContext;
            if (rexp != null)
            {
                var res = match_rexp(p_child, t_child, map);
                if (res && map) _matches.MyAdd(t, p);
                return res;
            }
            SpecParserParser.AttrContext attr = p_child as SpecParserParser.AttrContext;
            if (attr != null)
            {
                var res = match_attr(p_child, t_child, map);
                if (res && map) _matches.MyAdd(t, p);
                return res;
            }
            return false;
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

        /*
          *
          * pattern grammar--
          * attr: ID EQ (StringLiteral | STAR);
          *
          * tree grammar--
          * attr : ID EQUALS StringLiteral ;
          *
          * Add entry into matches t => p if there is a match and map is true.
          */
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
                if (map) _matches.MyAdd(t, p);
                return true;
            }

            if (pattern.StartsWith("$\""))
            {
                pattern = pattern.Substring(2);
                pattern = pattern.Substring(0, pattern.Length - 1);
                try
                {
                    pattern = ReplaceMacro(p);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Cannot perform substitution in pattern with string.");
                    System.Console.WriteLine("String " + pattern);
                    System.Console.WriteLine("Pattern " + TreeRegEx.sourceTextForContext(p));
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
            if (result && map) _matches.MyAdd(t, p);
            return result;
        }
    }
}
