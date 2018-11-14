using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;

namespace Piggy
{
    public class TreeRegEx
    {
        // Pattern matcher.

        private IParseTree current_ast_node;
        private Stack<IParseTree> matches = new Stack<IParseTree>();

        public List<IParseTree> dfs_match(IParseTree t, IParseTree start)
        {
            List<IParseTree> matches = new List<IParseTree>();
            var visited = new HashSet<IParseTree>();
            var stack = new Stack<IParseTree>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var vertex = stack.Pop();

                if (visited.Contains(vertex))
                    continue;

                visited.Add(vertex);

                // Try matching at vertex.
                bool matched = match_template(t, vertex);
                if (matched) matches.Add(vertex);

                for (int i = vertex.ChildCount - 1; i >= 0; --i)
                {
                    var neighbor = vertex.GetChild(i);
                    if (!visited.Contains(neighbor))
                        stack.Push(neighbor);
                }
            }

            return matches;
        }

        /* Match template: TEMPLATE rexp SEMI ;
         */
        public bool match_template(IParseTree p, IParseTree t)
        {
            SpecParserParser.TemplateContext template =
                p as SpecParserParser.TemplateContext;
            if (template == null) return false;
            var re = p.GetChild(1);
            if (re == null) return false;
            return match_rexp(re, t);
        }

        /* Match rexp : simple_rexp (OR simple_rexp)* ;
        */
        bool match_rexp(IParseTree p, IParseTree t)
        {
            SpecParserParser.RexpContext re =
                p as SpecParserParser.RexpContext;
            if (re == null) return false;
            int pos = 0;
            IParseTree start = re.GetChild(0);
            if (match_simple_re(start, t)) return true;
            for (; ; )
            {
                pos += 2;
                start = re.GetChild(pos);
                if (start == null) break;
                if (match_simple_re(start, t)) return true;
            }
            return false;
        }

        /* Match simple_rexp : basic_rexp+ ;
        */
        bool match_simple_re(IParseTree p, IParseTree t)
        {
            SpecParserParser.Simple_rexpContext simple_re =
                p as SpecParserParser.Simple_rexpContext;
            if (simple_re == null) return false;
            int pos = 0;
            IParseTree start = simple_re.GetChild(0);
            if (!match_basic_re(start, t)) return false;
            for (; ; )
            {
                pos += 1;
                start = simple_re.GetChild(pos);
                if (start == null) break;
                if (!match_basic_re(start, t)) return false;
            }
            return true;
        }

        /* Match basic_rexp : star_rexp | plus_rexp | elementary_rexp ;
         */
        bool match_basic_re(IParseTree p, IParseTree t)
        {
            SpecParserParser.Basic_rexpContext basic_re =
                p as SpecParserParser.Basic_rexpContext;
            if (basic_re == null) return false;
            var child = basic_re.GetChild(0);
            if (child == null) return false;

            SpecParserParser.Star_rexpContext star_rexp =
                child as SpecParserParser.Star_rexpContext;
            if (star_rexp != null)
                return match_star_rexp(star_rexp, t);
            SpecParserParser.Plus_rexpContext plus_rexp =
                child as SpecParserParser.Plus_rexpContext;
            if (plus_rexp != null)
                return match_plus_rexp(plus_rexp, t);

            return match_elementary_rexp(child, t);
        }

        /*
         * star_rexp: elementary_rexp STAR;
         */
        bool match_star_rexp(IParseTree p, IParseTree t)
        {
            SpecParserParser.Star_rexpContext star_rexp =
                p as SpecParserParser.Star_rexpContext;
            if (star_rexp == null) return false;
            var child = star_rexp.GetChild(0);
            if (child == null) return false;
            // Match zero or more of elementary.
            var result = false;
            for (; ; )
            {
                bool b = match_elementary_rexp(child, t);
                if (!b) break;
                result = true;
                // advance...
            }
            return result;
        }

        /*
         * plus_rexp: elementary_rexp PLUS;
         */
        bool match_plus_rexp(IParseTree p, IParseTree t)
        {
            SpecParserParser.Star_rexpContext star_rexp =
                p as SpecParserParser.Star_rexpContext;
            if (star_rexp == null) return false;
            var child = star_rexp.GetChild(0);
            if (child == null) return false;
            // Match zero or more of elementary.
            var result = true;
            for (; ; )
            {
                bool b = match_elementary_rexp(child, t);
                if (!b) break;
                result = true;
                // advance...
            }
            return result;
        }

        /*
         * elementary_rexp: group_rexp | basic ;
         */
        bool match_elementary_rexp(IParseTree p, IParseTree t)
        {
            SpecParserParser.Elementary_rexpContext elementary_rexp =
                p as SpecParserParser.Elementary_rexpContext;
            if (elementary_rexp == null) return false;
            var child = elementary_rexp.GetChild(0);
            if (child == null) return false;
            SpecParserParser.Group_rexpContext group_rexp =
                child as SpecParserParser.Group_rexpContext;
            if (group_rexp != null)
                return match_group_rexp(group_rexp, t);
            SpecParserParser.BasicContext basic =
                child as SpecParserParser.BasicContext;
            if (basic != null)
                return match_basic(basic, t);

            return false;
        }

        /*
         * group_rexp:   OPEN_PAREN rexp CLOSE_PAREN ;
         */
        bool match_group_rexp(IParseTree p, IParseTree t)
        {
            SpecParserParser.Group_rexpContext group_rexp =
                p as SpecParserParser.Group_rexpContext;
            if (group_rexp == null) return false;
            var child = group_rexp.GetChild(1);
            if (child == null) return false;
            return match_rexp(child, t);
        }

        /*
         * basic: OPEN_RE ID more* CLOSE_RE ;
         *
         * decl : OPEN_PAREN ID more* CLOSE_PAREN ;
         */
        bool match_basic(IParseTree p, IParseTree t)
        {
            SpecParserParser.BasicContext basic =
                p as SpecParserParser.BasicContext;
            if (basic == null) return false;

            // Match open paren.
            int pos = 0;
            var decl = t as AstParserParser.DeclContext;
            if (decl == null) return false;
            var t_c = decl.GetChild(pos);
            var t_tok = t_c as TerminalNodeImpl;
            if (t_tok == null) return false;
            var t_sym = t_tok.Symbol;
            if (t_sym.Type != AstLexer.OPEN_PAREN) return false;
            var p_c = basic.GetChild(pos);
            var p_tok = p_c as TerminalNodeImpl;
            if (p_tok == null) return false;
            var p_sym = p_tok.Symbol;
            if (p_sym.Type != SpecParserParser.OPEN_RE) return false;

            pos++;

            // Match ID.
            var id_tree = decl.GetChild(pos);
            var id = basic.GetChild(pos);
            if (id.GetText() != id_tree.GetText()) return false;

            pos++;

            // We are now at "more" in both t and p.
            // p contains list of "more", as well as t.
            // p's items should all match t's in order.

            int p_pos = pos;
            int t_pos = pos;
            IParseTree p_more = null;
            IParseTree t_more = null;
            bool result = true;
            for (; ; )
            {
                for (; ; )
                {
                    p_more = basic.GetChild(p_pos);
                    if (p_more == null) break;
                    if (!(p_more as SpecParserParser.CodeContext != null
                        || p_more as SpecParserParser.TextContext != null))
                        break;
                    p_pos++;
                }
                if (p_more == null) break;
                t_more = decl.GetChild(t_pos);
                if (t_more == null)
                {
                    result = false;
                    break;
                }

                SpecParserParser.MoreContext c11 =
                    p_more as SpecParserParser.MoreContext;
                if (c11 == null)
                {
                    result = false;
                    break;
                }
                AstParserParser.MoreContext c22 =
                    t_more as AstParserParser.MoreContext;
                if (c22 == null) break;

                var p_child = p_more.GetChild(0);
                if (p_child as SpecParserParser.CodeContext != null
                    || p_child as SpecParserParser.TextContext != null)
                {
                    p_pos++;
                    continue;
                }

                if (match_more(c11, c22))
                {
                    // advance both.
                    p_pos++;
                    t_pos++;
                    continue;
                }
                // advance t.
                t_pos++;
            }

            if (t_more == null) return true;
            return false;
        }

        /* pattern grammar--
         * more : rexp | text | code | attr ;
         *
         * tree grammar--
         * more : decl | attr ;
         */
        bool match_more(IParseTree p, IParseTree t)
        {
            SpecParserParser.MoreContext p_more =
                p as SpecParserParser.MoreContext;
            if (p_more == null) return false;

            AstParserParser.MoreContext t_more =
                t as AstParserParser.MoreContext;
            if (t_more == null) return false;

            bool result = true;
            var p_child = p_more.GetChild(0);
            if (p_child as SpecParserParser.CodeContext != null
                || p_child as SpecParserParser.TextContext != null)
                return true;

            var t_child = t.GetChild(0);

            SpecParserParser.RexpContext rexp =
                p_child as SpecParserParser.RexpContext;
            if (rexp != null)
                return match_rexp(p_child, t_child);

            SpecParserParser.AttrContext attr =
                p_child as SpecParserParser.AttrContext;
            if (attr != null)
                return match_attr(p_child, t_child);

            return false;
        }

        /*
         * code: LCURLY OTHER* RCURLY ;
         */
        bool match_code(IParseTree p, IParseTree t)
        {
            SpecParserParser.CodeContext code =
                p as SpecParserParser.CodeContext;
            if (code == null) return false;
            return true;
        }

        /*
         * text: LANG OTHER_ANG* RANG ;
         */
        bool match_text(IParseTree p, IParseTree t)
        {
            SpecParserParser.TextContext text =
                p as SpecParserParser.TextContext;
            if (text == null) return false;
            return true;
        }

        /*
         * attr: ID EQ (StringLiteral | STAR);
         */
        bool match_attr(IParseTree p, IParseTree t)
        {
            SpecParserParser.AttrContext p_attr =
                p as SpecParserParser.AttrContext;
            if (p_attr == null) return false;
            AstParserParser.AttrContext t_attr =
                t as AstParserParser.AttrContext;
            if (t_attr == null) return false;

            int pos = 0;
            var p_id = p_attr.GetChild(pos);
            var t_id = t_attr.GetChild(pos);
            if (p_id.GetText() != t_id.GetText()) return false;

            pos++;
            pos++;

            var p_val = p_attr.GetChild(pos);
            var t_val = t_attr.GetChild(pos);
            if (p_val.GetText() == "*") return true;

            return p_val.GetText() == t_val.GetText();
        }
    }
}
