﻿namespace PiggyGenerator
{
    using System.Collections.Generic;
    using System.Text;
    using Antlr4.Runtime.Tree;

    /**
     * NFA construction via Thompson's Construction.
     */
    public class NFA
    {
        public List<State> _all_states = new List<State>();
        public List<Edge> _all_edges = new List<Edge>();
        public State _start_state = null;
        public List<State> _final_states = new List<State>();

        /**
         * Computes and returns the post-order nodes of a tree.
         */
        private static List<IParseTree> ComputePostOrder(IParseTree tree)
        {
            Stack<IParseTree> stack = new Stack<IParseTree>();
            HashSet<IParseTree> visited = new HashSet<IParseTree>();
            stack.Push(tree);
            var post_order = new List<IParseTree>();
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (visited.Contains(v))
                    post_order.Add(v);
                else
                {
                    stack.Push(v);
                    visited.Add(v);
                    for (int i = v.ChildCount - 1; i >= 0; --i)
                    {
                        var c = v.GetChild(i);
                        stack.Push(c);
                    }
                }
            }
            return post_order;
        }

        /**
         * Generates the NFA from the given tree pattern using Thompson's Construction.
         */
        public void post2nfa(IParseTree tree)
        {
            var post_order = ComputePostOrder(tree);
            Stack<Fragment> fragmentStack = new Stack<Fragment>();
            Fragment completeNfa = new Fragment();
            foreach (var p in post_order)
            {
                var p_text = p.GetText();
                var p_type = p.GetType();
                //System.Console.Error.WriteLine("Next p " + p_text + " " + p_type);

                // simple_basic, kleen_star_basic, and continued_basic are very special:
                // add in ".*" between each item.
                if (p as SpecParserParser.Simple_basicContext != null
                    || p as SpecParserParser.Kleene_star_basicContext != null
                    || p as SpecParserParser.Continued_basicContext != null)
                {
                    Fragment last = fragmentStack.Pop();
                    for (int i = p.ChildCount - 2; i >= 0; --i)
                    {
                        Fragment f = fragmentStack.Pop();
                        if (i > 0)
                        {
                            // Add in ".*"
                            State s1 = new State(this);
                            State s2 = new State(this);
                            State s3 = new State(this);
                            new Edge(this, s1, s2, null, null);
                            new Edge(this, s2, s3, null, null);
                            new Edge(this, s2, s2, null, null, (int)Edge.EdgeModifiers.Any);
                            new Edge(this, s3, last.StartState, null, null);
                            last = new Fragment(s1, last.OutStates);
                        }
                        foreach (var o in f.OutStates) new Edge(this, o, last.StartState, null, null);
                        last = new Fragment(f.StartState, last.OutStates);
                    }
                    fragmentStack.Push(last);
                }
                else if(p as SpecParserParser.BasicContext != null) { }
                else if (p as SpecParserParser.PatternContext != null) { }
                else if (p as TerminalNodeImpl != null)
                {
                    TerminalNodeImpl t = p as TerminalNodeImpl;
                    var s = t.Symbol;
                    var s_type = s.Type;
                    if (s.Type == SpecParserParser.OPEN_PAREN ||
                        s.Type == SpecParserParser.OPEN_KLEENE_STAR_PAREN ||
                        s.Type == SpecParserParser.OPEN_VISIT ||
                        s.Type == SpecParserParser.CLOSE_PAREN ||
                        s.Type == SpecParserParser.CLOSE_KLEENE_STAR_PAREN ||
                        s.Type == SpecParserParser.CLOSE_VISIT)
                    {
                        State s1 = new State(this);
                        State s2 = new State(this);
                        var e = new Edge(this, s1, s2, t, null);
                        var f = new Fragment(s1, s2);
                        fragmentStack.Push(f);
                    }
                }
                else if (p as SpecParserParser.Id_or_star_or_emptyContext != null)
                {
                    var c = p.GetChild(0);
                    State s1 = new State(this);
                    State s2 = new State(this);
                    var e = new Edge(this, s1, s2, c, null);
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.MoreContext != null) { }
                else if (p as SpecParserParser.TextContext != null)
                {
                    State s1 = new State(this);
                    State s2 = new State(this);
                    var e = new Edge(this, s1, s2, null, p);
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.CodeContext != null)
                {
                    State s1 = new State(this);
                    State s2 = new State(this);
                    var e = new Edge(this, s1, s2, null, p);
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.Group_rexpContext != null) { }
                else if (p as SpecParserParser.Star_rexpContext != null)
                {
                    Fragment previous = fragmentStack.Pop();
                    State s1 = new State(this);
                    var e1 = new Edge(this, s1, previous.StartState, null, null);
                    foreach (var s in previous.OutStates) new Edge(this, s, s1, null, null);
                    var f = new Fragment(s1, s1);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.Plus_rexpContext != null)
                {
                    Fragment previous = fragmentStack.Pop();
                    State s1 = new State(this);
                    State s2 = new State(this);
                    var e1 = new Edge(this, s1, s2, null, null);
                    var e2 = new Edge(this, s2, previous.StartState, null, null);
                    foreach (var s in previous.OutStates) new Edge(this, s, s2, null, null);
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.AttrContext != null)
                {
                    var c = p.GetChild(0);
                    if (c.GetText() == "!")
                    {
                        TerminalNodeImpl t = c as TerminalNodeImpl;
                        var s = t.Symbol;
                        var s_type = s.Type;
                        State s1 = new State(this);
                        State s2 = new State(this);
                        new Edge(this, s1, s2, t, null, (int)Edge.EdgeModifiers.Not);
                        var f = new Fragment(s1, s2);
                        fragmentStack.Push(f);
                    }
                    else
                    {
                        TerminalNodeImpl t = c as TerminalNodeImpl;
                        var s = t.Symbol;
                        var s_type = s.Type;
                        State s1 = new State(this);
                        State s2 = new State(this);
                        State s3 = new State(this);
                        State s4 = new State(this);
                        new Edge(this, s1, s2, t, null);
                        t = p.GetChild(1) as TerminalNodeImpl;
                        new Edge(this, s2, s3, t, null);
                        t = p.GetChild(2) as TerminalNodeImpl;
                        new Edge(this, s3, s4, t, null);
                        var f = new Fragment(s1, s4);
                        fragmentStack.Push(f);
                    }
                }
                else if (p as SpecParserParser.RexpContext != null)
                {
                    for (int i = 2; i < p.ChildCount; i += 2)
                    {
                        State s = new State(this);
                        Fragment s2 = fragmentStack.Pop();
                        Fragment s1 = fragmentStack.Pop();
                        var e1 = new Edge(this, s, s1.StartState, null, null);
                        var e2 = new Edge(this, s, s2.StartState, null, null);
                        State s3 = new State(this);
                        foreach (var o in s1.OutStates) new Edge(this, o, s3, null, null);
                        foreach (var o in s2.OutStates) new Edge(this, o, s3, null, null);
                        var f = new Fragment(s, s3);
                        fragmentStack.Push(f);
                    }
                }
            }

            completeNfa = fragmentStack.Pop();
            if (fragmentStack.Count > 0)
                throw new System.Exception("Fragment stack not empty.");

            _final_states = completeNfa.OutStates;
            foreach (var s in _final_states) s._match = true;
            _start_state = completeNfa.StartState;
            System.Console.Error.WriteLine(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("digraph g {");
            foreach (var e in _all_edges)
            {
                sb.Append(e._from + " -> " + e._to
                    + " [label=\"");
                if (e._other != null && e._other as SpecParserParser.TextContext != null)
                    sb.Append("[[ text ]]");
                else if (e._other != null && e._other as SpecParserParser.CodeContext != null)
                    sb.Append("{{ code }}");
                else if (e._any)
                    sb.Append(" any ");
                else if (e._c_text == null)
                    sb.Append(" empty ");
                else
                    sb.Append(e._c_text);
                sb.AppendLine("\"];");
            }
            sb.AppendLine(_start_state + " [shape=box];");
            foreach (var end in _final_states) sb.AppendLine(end + " [shape=doublecircle];");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
