using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Piggy
{
    public class Intercept<K, V> : Dictionary<K, V>
    {
        public void MyAdd(K k, V v)
        {
            this[k] = v;
        }

        public V this[K key]
        {
            get
            {
                return base[key];
            }
            set
            {
                var t = (IParseTree) key;
                var p = (IParseTree) value;
                //System.Console.WriteLine(
                //    String.Format("Adding match[{0}] = {1}",
                //        TreeRegEx.sourceTextForContext(t), TreeRegEx.sourceTextForContext(p)));
                base[key] = value;
            }
        }
    }

    public class TreeRegEx
    {
        public TreeRegEx(List<SpecParserParser.TemplateContext> t, IParseTree s)
        {
            _ast = s;
            bool result = false;
            var visited = new HashSet<IParseTree>();
            var stack = new Stack<IParseTree>();
            stack.Push(_ast);
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
            var x = context as Antlr4.Runtime.ParserRuleContext;
            if (x == null)
            {
                return "";
            }
            var c = x;
            IToken startToken = c.Start;
            IToken stopToken = c.Stop;
            ICharStream cs = startToken.InputStream;
            int stopIndex = stopToken.StopIndex;
            return cs.GetText(new Antlr4.Runtime.Misc.Interval(startToken.StartIndex, stopIndex));
        }

        public IParseTree _ast;
        public List<SpecParserParser.TemplateContext> templates;
        public Intercept<IParseTree, IParseTree> matches = new Intercept<IParseTree, IParseTree>();
        public Dictionary<IParseTree, int> depth = new Dictionary<IParseTree, int>();
        public Dictionary<IParseTree, int> pre_order_number = new Dictionary<IParseTree, int>();
        public List<IParseTree> pre_order = new List<IParseTree>();
        public List<IParseTree> post_order = new List<IParseTree>();
        public Dictionary<IParseTree, IParseTree> parent = new Dictionary<IParseTree, IParseTree>();

        public bool has_output(IParseTree p)
        {
            bool result = false;
            var visited = new HashSet<IParseTree>();
            var stack = new Stack<IParseTree>();
            stack.Push(p);
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (visited.Contains(v))
                    continue;
                visited.Add(v);
                if (v as SpecParserParser.TextContext != null
                    || v as SpecParserParser.CodeContext != null)
                    return true;
                for (int i = v.ChildCount - 1; i >= 0; --i)
                {
                    var c = v.GetChild(i);
                    if (!visited.Contains(c))
                        stack.Push(c);
                }
            }
            return false;
        }

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

        /* Match template: TEMPLATE rexp SEMI ;
         */
        private bool match_template(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.TemplateContext template =
                p as SpecParserParser.TemplateContext;
            if (template == null) return false;
            var re = p.GetChild(1);
            if (re == null) return false;
            bool result = match_rexp(re, t, map);
            //if (result && map && has_output(p)) matches[p] = t;
            return result;
        }

        /* Match rexp : simple_rexp (OR simple_rexp)* ;
        */
        private bool match_rexp(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.RexpContext re =
                p as SpecParserParser.RexpContext;
            if (re == null) return false;
            int pos = 0;
            IParseTree start = re.GetChild(0);
            bool result = match_simple_re(start, t, map);
            // if (result && map && has_output(_ast)) matches[_ast] = t;
            if (result) return true;
            for (; ; )
            {
                pos += 2;
                start = re.GetChild(pos);
                if (start == null) break;
                result = match_simple_re(start, t, map);
                //if (result && map && has_output(_ast)) matches[_ast] = t;
                if (match_simple_re(start, t, map)) return true;
            }
            return false;
        }

        /* Match simple_rexp : basic_rexp+ ;
        */
        private bool match_simple_re(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.Simple_rexpContext simple_re =
                p as SpecParserParser.Simple_rexpContext;
            if (simple_re == null) return false;
            int pos = 0;
            IParseTree start = simple_re.GetChild(0);
            var result = match_basic_re(start, t, map);
            // if (result && map && has_output(_ast)) matches[_ast] = t;
            if (!result) return false;
            for (; ; )
            {
                pos += 1;
                start = simple_re.GetChild(pos);
                if (start == null) break;
                result = match_basic_re(start, t, map);
                // if (result && map && has_output(_ast)) matches[_ast] = t;
                if (!result) return false;
            }
            return true;
        }

        /* Match basic_rexp : star_rexp | plus_rexp | elementary_rexp ;
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
                //if (result && map && has_output(star_rexp)) matches[star_rexp] = t;
                return result;
            }
            SpecParserParser.Plus_rexpContext plus_rexp =
                child as SpecParserParser.Plus_rexpContext;
            if (plus_rexp != null)
            {
                var result = match_plus_rexp(plus_rexp, t, map);
                //if (result && map && has_output(plus_rexp)) matches[plus_rexp] = t;
                return result;
            }
            {
                var result = match_elementary_rexp(child, t, map);
                // if (result && map && has_output(child)) matches[child] = t;
                return result;
            }
        }

        /*
         * star_rexp: elementary_rexp STAR;
         */
        private bool match_star_rexp(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.Star_rexpContext star_rexp =
                p as SpecParserParser.Star_rexpContext;
            if (star_rexp == null) return false;
            int pos = 0;
            var child = star_rexp.GetChild(0);
            if (child == null) return true;
            // Match zero or more of elementary. Note, we are matching
            // a elementary_rexp with a "more" type in the ast.
            bool b = match_elementary_rexp(child, t, map);
            return b;
        }

        /*
         * plus_rexp: elementary_rexp PLUS;
         */
        private bool match_plus_rexp(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.Star_rexpContext star_rexp =
                p as SpecParserParser.Star_rexpContext;
            if (star_rexp == null) return false;
            var child = star_rexp.GetChild(0);
            if (child == null) return false;
            // Match one or more of elementary.
            var result = true;
            bool b = match_elementary_rexp(child, t, map);
            return b;
        }

        /*
         * elementary_rexp: group_rexp | basic ;
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
                //if (result && map && has_output(group_rexp)) matches[group_rexp] = t;
                return result;
            }
            SpecParserParser.BasicContext basic =
                child as SpecParserParser.BasicContext;
            if (basic != null)
            {
                var result = match_basic(basic, t, map);
                // if (result && map && has_output(basic)) matches[basic] = t;
                return result;
            }

            return false;
        }

        /*
         * group_rexp:   OPEN_PAREN rexp CLOSE_PAREN ;
         */
        private bool match_group_rexp(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.Group_rexpContext group_rexp =
                p as SpecParserParser.Group_rexpContext;
            if (group_rexp == null) return false;
            var child = group_rexp.GetChild(1);
            if (child == null) return false;
            var result = match_rexp(child, t, map);
            //if (result && map && has_output(child)) matches[child] = t;
            return result;
        }

        /*
         * Determine via lookahead if a node for a pattern matcher
         * is an attribute or not.
         */
        public bool is_pattern_attr(IParseTree p)
        {
            if (p == null) return false;
            SpecParserParser.AttrContext attr =
                p as SpecParserParser.AttrContext;
            return attr != null;
        }

        /*
         * Determine via lookahead if a node for a pattern matcher
         * is an attribute or not.
         */
        public bool is_ast_attr(IParseTree p)
        {
            if (p == null) return false;
            AstParserParser.AttrContext attr =
                p as AstParserParser.AttrContext;
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

        /*
         * basic: OPEN_PAREN ID more* CLOSE_PAREN ;
         *
         * decl : OPEN_PAREN ID more* CLOSE_PAREN ;
         */
        private bool match_basic(IParseTree p, IParseTree t, bool map = false)
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
            if (p_sym.Type != SpecParserParser.OPEN_PAREN) return false;

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
            int not_counting_parens = 1;
            for ( ; p_pos < basic.ChildCount - not_counting_parens; ++p_pos)
            {
                p_more = basic.GetChild(p_pos);
                if (p_more == null) break;
                if (p_more as SpecParserParser.CodeContext != null
                    || p_more as SpecParserParser.TextContext != null)
                    continue;
                SpecParserParser.MoreContext c11 =
                    p_more as SpecParserParser.MoreContext;
                if (c11 == null) return false;
                var p_child = p_more.GetChild(0);
                if (p_child as SpecParserParser.CodeContext != null
                    || p_child as SpecParserParser.TextContext != null)
                    continue;

                // If this element of the pattern is an attribute,
                // go through all previous elements of t.
                bool is_attr = this.is_pattern_attr(p_more.GetChild(0));
                bool matched = false;
                for (int j = is_attr ? 2 : t_pos; j < decl.ChildCount - not_counting_parens; ++j)
                {
                    t_more = decl.GetChild(j);
                    AstParserParser.MoreContext c22 =
                        t_more as AstParserParser.MoreContext;
                    if (c22 == null) return false;
                    if (match_more(c11, c22, map))
                    {
                        matched = true;
                        t_pos = j;
                        //break;
                    }
                }
                if (!matched) return false;
            }

            if (true && map && has_output(p)) matches[t] = p;
            return true;
        }

        /* pattern grammar--
         * more : rexp | text | code | attr ;
         *
         * tree grammar--
         * more : decl | attr ;
         */
        private bool match_more(IParseTree p, IParseTree t, bool map = false)
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
            {
                var res = match_rexp(p_child, t_child, map);
                //if (res && map && has_output(p_child)) matches[p_child] = t_child;
                return res;
            }

            SpecParserParser.AttrContext attr =
                p_child as SpecParserParser.AttrContext;
            if (attr != null)
            {
                var res = match_attr(p_child, t_child, map);
                return res;
            }

            return false;
        }

        /*
         * code: LCURLY OTHER* RCURLY ;
         */
        private bool match_code(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.CodeContext code =
                p as SpecParserParser.CodeContext;
            if (code == null) return false;
            return true;
        }

        /*
         * text: LANG OTHER_ANG* RANG ;
         */
        private bool match_text(IParseTree p, IParseTree t, bool map = false)
        {
            SpecParserParser.TextContext text =
                p as SpecParserParser.TextContext;
            if (text == null) return false;
            return true;
        }

        /*
         * attr: ID EQ (StringLiteral | STAR);
         */
        private bool match_attr(IParseTree p, IParseTree t, bool map = false)
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
