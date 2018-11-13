using System;
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

                for (int i = 0; i < vertex.ChildCount; ++i)
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
        public bool match_template(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.TemplateContext template = pattern_node as SpecParserParser.TemplateContext;
            if (template == null) return false;
            var re = pattern_node.GetChild(1);
            if (re == null) return false;
            return match_rexp(re, current_node);
        }

        /* Match rexp : simple_rexp (OR simple_rexp)* ;
        */
        bool match_rexp(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.RexpContext re = pattern_node as SpecParserParser.RexpContext;
            if (re == null) return false;
            int pos = 0;
            IParseTree start = re.GetChild(0);
            if (match_simple_re(start, current_node)) return true;
            for (; ; )
            {
                pos += 2;
                start = re.GetChild(pos);
                if (start == null) break;
                if (match_simple_re(start, current_node)) return true;
            }
            return false;
        }

        /* Match simple_rexp : basic_rexp+ ;
        */
        bool match_simple_re(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.Simple_rexpContext simple_re = pattern_node as SpecParserParser.Simple_rexpContext;
            if (simple_re == null) return false;
            int pos = 0;
            IParseTree start = simple_re.GetChild(0);
            if (!match_basic_re(start, current_node)) return false;
            for (; ; )
            {
                pos += 1;
                start = simple_re.GetChild(pos);
                if (start == null) break;
                if (!match_basic_re(start, current_node)) return false;
            }
            return true;
        }

        /* Match basic_rexp : star_rexp | plus_rexp | elementary_rexp ;
         */
        bool match_basic_re(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.Basic_rexpContext basic_re = pattern_node as SpecParserParser.Basic_rexpContext;
            if (basic_re == null) return false;
            var child = basic_re.GetChild(0);
            if (child == null) return false;

            SpecParserParser.Star_rexpContext star_rexp = child as SpecParserParser.Star_rexpContext;
            if (star_rexp != null)
                return match_star_rexp(star_rexp, current_node);
            SpecParserParser.Plus_rexpContext plus_rexp = child as SpecParserParser.Plus_rexpContext;
            if (plus_rexp != null)
                return match_plus_rexp(plus_rexp, current_node);

            return match_elementary_rexp(child, current_node);
        }

        /*
         * star_rexp: elementary_rexp STAR;
         */
        bool match_star_rexp(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.Star_rexpContext star_rexp = pattern_node as SpecParserParser.Star_rexpContext;
            if (star_rexp == null) return false;
            var child = star_rexp.GetChild(0);
            if (child == null) return false;
            // Match zero or more of elementary.
            var result = false;
            for (; ; )
            {
                bool p = match_elementary_rexp(child, current_node);
                if (!p) break;
                result = true;
                // advance...
            }
            return result;
        }

        /*
         * plus_rexp: elementary_rexp PLUS;
         */
        bool match_plus_rexp(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.Star_rexpContext star_rexp = pattern_node as SpecParserParser.Star_rexpContext;
            if (star_rexp == null) return false;
            var child = star_rexp.GetChild(0);
            if (child == null) return false;
            // Match zero or more of elementary.
            var result = true;
            for (; ; )
            {
                bool p = match_elementary_rexp(child, current_node);
                if (!p) break;
                result = true;
                // advance...
            }
            return result;
        }

        /*
         * elementary_rexp: group_rexp | basic ;
         */
        bool match_elementary_rexp(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.Elementary_rexpContext elementary_rexp = pattern_node as SpecParserParser.Elementary_rexpContext;
            if (elementary_rexp == null) return false;
            var child = elementary_rexp.GetChild(0);
            if (child == null) return false;
            SpecParserParser.Group_rexpContext group_rexp = child as SpecParserParser.Group_rexpContext;
            if (group_rexp != null)
                return match_group_rexp(group_rexp, current_node);
            SpecParserParser.BasicContext basic = child as SpecParserParser.BasicContext;
            if (basic != null)
                return match_basic(basic, current_node);

            return false;
        }

        /*
         * group_rexp:   OPEN_PAREN rexp CLOSE_PAREN ;
         */
        bool match_group_rexp(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.Group_rexpContext group_rexp = pattern_node as SpecParserParser.Group_rexpContext;
            if (group_rexp == null) return false;
            var child = group_rexp.GetChild(1);
            if (child == null) return false;
            return match_rexp(group_rexp, current_node);
        }

        /*
         * basic: OPEN_RE ID more* CLOSE_RE ;
         */
        bool match_basic(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.BasicContext basic = pattern_node as SpecParserParser.BasicContext;
            if (basic == null) return false;
            var id = basic.GetChild(1);
            int pos = 1;
            for (; ; )
            {
                pos++;
                var c = basic.GetChild(pos);
                SpecParserParser.MoreContext more = c as SpecParserParser.MoreContext;
                if (more == null) break;
                if (!match_more(more, current_node)) return false;
            }
            return true;
        }

        /*
         * more : rexp | text | code | attr ;
         */
        bool match_more(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.MoreContext more = pattern_node as SpecParserParser.MoreContext;
            if (more == null) return false;
            var child = more.GetChild(0);
            if (child == null) return false;

            SpecParserParser.RexpContext rexp = child as SpecParserParser.RexpContext;
            if (rexp != null)
                return match_rexp(rexp, current_node);
            SpecParserParser.TextContext text = child as SpecParserParser.TextContext;
            if (text != null)
                return match_text(text, current_node);
            SpecParserParser.CodeContext code = child as SpecParserParser.CodeContext;
            if (code != null)
                return match_code(code, current_node);
            SpecParserParser.AttrContext attr = child as SpecParserParser.AttrContext;
            if (attr != null)
                return match_attr(attr, current_node);

            return false;
        }

        /*
         * code: LCURLY OTHER* RCURLY ;
         */
        bool match_code(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.CodeContext code = pattern_node as SpecParserParser.CodeContext;
            if (code == null) return false;
            return true;
        }

        /*
         * text: LANG OTHER_ANG* RANG ;
         */
        bool match_text(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.TextContext text = pattern_node as SpecParserParser.TextContext;
            if (text == null) return false;
            return true;
        }

        /*
         * attr: ID EQ (StringLiteral | STAR);
         */
        bool match_attr(IParseTree pattern_node, IParseTree current_node)
        {
            SpecParserParser.AttrContext attr = pattern_node as SpecParserParser.AttrContext;
            if (attr == null) return false;
            return true;
        }
    }
}
