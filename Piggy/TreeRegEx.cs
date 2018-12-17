using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Text.RegularExpressions;

namespace Piggy
{
    public class TreeRegEx
    {
        public TreeRegEx(List<SpecParserParser.TemplateContext> t, IParseTree s)
        {
            _ast = s;
            bool result = false;
            var visited = new HashSet<IParseTree>();
            var stack = new Stack<IParseTree>();
            parent = Parents.Compute(_ast);

            templates = t;
            foreach (var te in templates)
            {
                stack.Push(te);
                while (stack.Count > 0)
                {
                    var v = stack.Pop();
                    if (visited.Contains(v))
                        continue;
                    visited.Add(v);
                    for (int i = v.ChildCount - 1; i >= 0; --i)
                    {
                        var c = v.GetChild(i);
                        parent[c] = v;
                        if (!visited.Contains(c))
                            stack.Push(c);
                    }
                }
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

        public IParseTree _ast;
        public List<SpecParserParser.TemplateContext> templates;
        public Intercept<IParseTree, IParseTree> matches = new Intercept<IParseTree, IParseTree>();
        public Dictionary<IParseTree, int> depth = new Dictionary<IParseTree, int>();
        public Dictionary<IParseTree, int> pre_order_number = new Dictionary<IParseTree, int>();
        public List<IParseTree> pre_order = new List<IParseTree>();
        public List<IParseTree> post_order = new List<IParseTree>();
        public Dictionary<IParseTree, IParseTree> parent = new Dictionary<IParseTree, IParseTree>();

        public void dfs_match()
        {
            var visited = new HashSet<IParseTree>();
            var stack = new Stack<IParseTree>();
            pre_order = new List<IParseTree>();
            stack.Push(_ast);
            depth[_ast] = 0;
            int current_dfs_number = 0;

            while (stack.Count > 0)
            {
                var v = stack.Pop();
                var current_depth = depth[v];
                if (visited.Contains(v)) continue;
                visited.Add(v);
                pre_order_number[v] = current_dfs_number;
                pre_order.Add(v);
                for (int i = v.ChildCount - 1; i >= 0; --i)
                {
                    var c = v.GetChild(i);
                    depth[c] = current_depth + 1;
                    if (!visited.Contains(c))
                        stack.Push(c);
                }
            }

            // Do pre-order walk to find matches.
            var copy = new Stack<IParseTree>(pre_order);
            post_order = new List<IParseTree>();
            while (copy.Any())
            {
                var x = copy.Pop();
                post_order.Add(x);
            }

            foreach (var v in pre_order)
            {
                foreach (SpecParserParser.TemplateContext t in templates)
                {
                    // Try matching at vertex, if the node hasn't been already matched.
                    if (!matches.ContainsKey(t))
                    {
                        //System.Console.WriteLine("Trying match ");
                        //System.Console.WriteLine("Template " + sourceTextForContext(t));
                        //System.Console.WriteLine("Tree " + sourceTextForContext(v));
                        bool matched = match_template(t, v);
                        if (matched)
                            match_template(t, v, true);
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
                    if (map) matches.MyAdd(t, p);
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
         * Match template: TEMPLATE rexp SEMI ;
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_template(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.TemplateContext template = p as SpecParserParser.TemplateContext;
            if (template == null) return false;
            var re = p.GetChild(0);
            if (re == null) return false;
            bool result = match_rexp(re, t, map);
            if (result && map) matches.MyAdd(t, p);
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
            if (result && map) matches.MyAdd(t, p);
            if (result) return true;
            for (; ; )
            {
                pos += 2;
                start = re.GetChild(pos);
                if (start == null) break;
                result = match_simple_re(start, t, map);
                if (result && map) matches.MyAdd(t, p);
                if (result) return true;
            }
            return false;
        }

        /* Match simple_rexp : basic_rexp+ ;
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
            for (; ; )
            {
                pos += 1;
                start = simple_re.GetChild(pos);
                if (start == null) break;
                result = match_basic_re(start, t, map);
                if (!result) return false; // If any non-match, the whole is non-match.
            }
            if (map) matches.MyAdd(t, p);
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
                if (result && map) matches.MyAdd(t, p);
                return result;
            }
            SpecParserParser.Plus_rexpContext plus_rexp =
                child as SpecParserParser.Plus_rexpContext;
            if (plus_rexp != null)
            {
                var result = match_plus_rexp(plus_rexp, t, map);
                if (result && map) matches.MyAdd(t, p);
                return result;
            }
            {
                var result = match_elementary_rexp(child, t, map);
                if (result && map) matches.MyAdd(t, p);
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
                if (map) matches.MyAdd(t, p);
                return true;
            }
            // Match zero or more of elementary. Note, we are matching
            // a elementary_rexp with a "more" type in the _display_ast.
            bool result = match_elementary_rexp(child, t, map);
            if (result && map) matches.MyAdd(t, p);
            return result;
        }

        /*
         * plus_rexp: elementary_rexp PLUS;
         * Add entry into matches t => p if there is a match and map is true.
         */
        private bool match_plus_rexp(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.Star_rexpContext star_rexp =
                p as SpecParserParser.Star_rexpContext;
            if (star_rexp == null) return false;
            var child = star_rexp.GetChild(0);
            if (child == null) return false;
            // Match one or more of elementary.
            bool result = match_elementary_rexp(child, t, map);
            if (result && map) matches.MyAdd(t, p);
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
                if (result && map) matches.MyAdd(t, p);
                return result;
            }
            SpecParserParser.BasicContext basic = child as SpecParserParser.BasicContext;
            if (basic != null)
            {
                var result = match_basic(basic, t, map);
                if (result && map) matches.MyAdd(t, p);
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
            if (result && map) matches.MyAdd(t, p);
            return result;
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
            if (result && map) matches.MyAdd(t, p);
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
                    p_sym.Type == SpecParserParser.OPEN_KLEENE_STAR_PAREN))
                    return false;

                if (map) matches.MyAdd(t_c, p_c);
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
                if (map) matches.MyAdd(id_tree, id);
                p_pos++;
                t_pos++;
            }
            else if (id.GetText() != id_tree.GetText())
                return false;
            else
            {
                if (map) matches.MyAdd(id_tree, id);
                t_pos++;
                p_pos++;
            }
            if (map)
            {
                matches.MyAdd(id_tree, id_or_star);
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
            for ( ; p_pos < p.ChildCount - not_counting_parens; ++p_pos)
            {
                p_more = p.GetChild(p_pos);
                if (p_more == null) break;
                var p_more_type = p_more.GetType();
                var p_more_text = p_more.GetText();
                if (p_more as SpecParserParser.CodeContext != null
                    || p_more as SpecParserParser.TextContext != null)
                    continue;
                SpecParserParser.MoreContext c11 = p_more as SpecParserParser.MoreContext;
                if (c11 == null) return false;
                var p_child = p_more.GetChild(0);
                if (p_child as SpecParserParser.CodeContext != null
                    || p_child as SpecParserParser.TextContext != null)
                    continue;
                var p_child_type = p_child.GetType();

                // Note order of attributes is significant.
                // Go through _display_ast and look for pattern in the _display_ast. If we
                // found any match, then record this point in the _display_ast matched.
                // Continue matching until the end of the _display_ast, which ends in
                // a ")". Note, if the pattern is a "*" or "+" expression,
                // we keep looking for this pattern. Otherwise, we skip to the
                // next pattern child.
                bool is_attr = this.is_pattern_attr(p_more);
                bool is_plus = this.is_pattern_plus(p_more);
                bool is_star = this.is_pattern_star(p_more);
                bool matched = false;
                //for (int j = is_attr ? 2 : t_pos; j < decl.ChildCount - not_counting_parens; ++j)
                for (int j = t_pos; j < decl.ChildCount - not_counting_parens; ++j)
                {
                    t_more = decl.GetChild(j);
                    var t_more_type = t_more.GetType();
                    var t_more_text = t_more.GetText();
                    AstParserParser.MoreContext c22 = t_more as AstParserParser.MoreContext;
                    if (c22 == null)
                        return false;
                    if (match_more(c11, c22, map))
                    {
                        // Current _display_ast child matches.
                        matched = true;
                        t_pos = j;
                    }
                    // Determine pattern child type. If it's a * or + grouping,
                    // continue to look for this pattern. Otherwise, move onto the next
                    // pattern child.
                    if (matched && !(is_plus || is_star))
                    {
                        break;
                    }
                }
                // If you didn't match pattern child, then this pattern can't match.
                if (!matched) return false;
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
                      || p_sym.Type == SpecParserParser.CLOSE_KLEENE_STAR_PAREN))
                    return false;
                if (map) matches.MyAdd(t_c, p_c);
            }

            if (true && map) matches.MyAdd(t, p);
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
                if (map) matches.MyAdd(t, p);
                return true;
            }
            var t_child = t.GetChild(0);
            SpecParserParser.RexpContext rexp = p_child as SpecParserParser.RexpContext;
            if (rexp != null)
            {
                var res = match_rexp(p_child, t_child, map);
                if (res && map) matches.MyAdd(t, p);
                return res;
            }
            SpecParserParser.AttrContext attr = p_child as SpecParserParser.AttrContext;
            if (attr != null)
            {
                var res = match_attr(p_child, t_child, map);
                if (res && map) matches.MyAdd(t, p);
                return res;
            }
            return false;
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
            if (p_id.GetText() != t_id.GetText()) return false;
            pos++;
            pos++;
            var p_val = p_attr.GetChild(pos);
            var t_val = t_attr.GetChild(pos);
            if (p_val.GetText() == "*")
            {
                if (map) matches.MyAdd(t, p);
                return true;
            }
            Regex re = new Regex(p_val.GetText());
            var matched = re.Match(t_val.GetText());
            var result = matched.Success;
            if (result && map) matches.MyAdd(t, p);
            return result;
        }
    }
}
