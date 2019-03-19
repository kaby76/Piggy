namespace PiggyGenerator
{
    using System.Collections.Generic;
    using System.Text;
    using Antlr4.Runtime.Tree;

    /**
     * NFA via Thompson's Construction.
     */
    public class NFA : Automaton
    {
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
        public static Automaton post2nfa(IParseTree tree)
        {
            var nfa = new Automaton();
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
                            State s1 = new State(nfa); s1.Commit();
                            State s2 = new State(nfa); s2.Commit();
                            State s3 = new State(nfa); s3.Commit();
                            var e1 = new Edge(nfa, s1, s2, null, null); e1.Commit();
                            var e2 = new Edge(nfa, s2, s3, null, null); e2.Commit();
                            var e3 = new Edge(nfa, s2, s2, null, null, (int)Edge.EdgeModifiers.Any); e3.Commit();
                            var e4 = new Edge(nfa, s3, last.StartState, null, null); e4.Commit();
                            last = new Fragment(s1, last.OutStates);
                        }
                        foreach (var o in f.OutStates)
                        {
                            var e5 = new Edge(nfa, o, last.StartState, null, null); e5.Commit();
                        }
                        last = new Fragment(f.StartState, last.OutStates);
                    }
                    fragmentStack.Push(last);
                }
                else if (p as SpecParserParser.BasicContext != null) { }
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
                        State s1 = new State(nfa); s1.Commit();
                        State s2 = new State(nfa); s2.Commit();
                        var e = new Edge(nfa, s1, s2, t, null); e.Commit();
                        var f = new Fragment(s1, s2);
                        fragmentStack.Push(f);
                    }
                }
                else if (p as SpecParserParser.Id_or_star_or_emptyContext != null)
                {
                    var c = p.GetChild(0);
                    State s1 = new State(nfa); s1.Commit();
                    State s2 = new State(nfa); s2.Commit();
                    var e = new Edge(nfa, s1, s2, c, null); e.Commit();
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.MoreContext != null) { }
                else if (p as SpecParserParser.TextContext != null)
                {
                    State s1 = new State(nfa); s1.Commit();
                    State s2 = new State(nfa); s2.Commit();
                    var e = new Edge(nfa, s1, s2, null, p); e.Commit();
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.CodeContext != null)
                {
                    State s1 = new State(nfa); s1.Commit();
                    State s2 = new State(nfa); s2.Commit();
                    var e = new Edge(nfa, s1, s2, null, p); e.Commit();
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.Group_rexpContext != null) { }
                else if (p as SpecParserParser.Star_rexpContext != null)
                {
                    Fragment previous = fragmentStack.Pop();
                    State s1 = new State(nfa); s1.Commit();
                    var e1 = new Edge(nfa, s1, previous.StartState, null, null); e1.Commit();
                    foreach (var s in previous.OutStates)
                    {
                        var e2 = new Edge(nfa, s, s1, null, null);
                        e2.Commit();
                    }
                    var f = new Fragment(s1, s1);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.Plus_rexpContext != null)
                {
                    Fragment previous = fragmentStack.Pop();
                    State s1 = new State(nfa); s1.Commit();
                    State s2 = new State(nfa); s2.Commit();
                    var e1 = new Edge(nfa, s1, s2, null, null); e1.Commit();
                    var e2 = new Edge(nfa, s2, previous.StartState, null, null); e2.Commit();
                    foreach (var s in previous.OutStates)
                    {
                        var e3 = new Edge(nfa, s, s2, null, null);
                        e3.Commit();
                    }
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
                        State s1 = new State(nfa); s1.Commit();
                        State s2 = new State(nfa); s2.Commit();
                        var e = new Edge(nfa, s1, s2, t, null, (int)Edge.EdgeModifiers.Not); e.Commit();
                        var f = new Fragment(s1, s2);
                        fragmentStack.Push(f);
                    }
                    else
                    {
                        TerminalNodeImpl t = c as TerminalNodeImpl;
                        var s = t.Symbol;
                        var s_type = s.Type;
                        State s1 = new State(nfa); s1.Commit();
                        State s2 = new State(nfa); s2.Commit();
                        State s3 = new State(nfa); s3.Commit();
                        State s4 = new State(nfa); s4.Commit();
                        var e1 = new Edge(nfa, s1, s2, t, null); e1.Commit();
                        t = p.GetChild(1) as TerminalNodeImpl;
                        var e2 = new Edge(nfa, s2, s3, t, null); e2.Commit();
                        t = p.GetChild(2) as TerminalNodeImpl;
                        var e3 = new Edge(nfa, s3, s4, t, null); e3.Commit();
                        var f = new Fragment(s1, s4);
                        fragmentStack.Push(f);
                    }
                }
                else if (p as SpecParserParser.RexpContext != null)
                {
                    for (int i = 2; i < p.ChildCount; i += 2)
                    {
                        State s = new State(nfa); s.Commit();
                        Fragment s2 = fragmentStack.Pop();
                        Fragment s1 = fragmentStack.Pop();
                        var e1 = new Edge(nfa, s, s1.StartState, null, null); e1.Commit();
                        var e2 = new Edge(nfa, s, s2.StartState, null, null); e2.Commit();
                        State s3 = new State(nfa); s3.Commit();
                        foreach (var o in s1.OutStates)
                        {
                            var e3 = new Edge(nfa, o, s3, null, null);
                            e3.Commit();
                        }
                        foreach (var o in s2.OutStates)
                        {
                            var e3 = new Edge(nfa, o, s3, null, null);
                            e3.Commit();
                        }
                        var f = new Fragment(s, s3);
                        fragmentStack.Push(f);
                    }
                }
            }
            completeNfa = fragmentStack.Pop();
            if (fragmentStack.Count > 0)
                throw new System.Exception("Fragment stack not empty.");
            foreach (var s in completeNfa.OutStates) nfa.AddEndState(s);
            foreach (var s in nfa.EndStates) s._match = true;
            nfa.AddStartState(completeNfa.StartState);
            System.Console.Error.WriteLine(nfa);
            return nfa;
        }
    }
}
