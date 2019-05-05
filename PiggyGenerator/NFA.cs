using System.Linq;

namespace PiggyGenerator
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Tree;
    using PiggyRuntime;

    /**
     * NFA via Thompson's Construction.
     */
    public class NFA
    {
        private State _start_state = null;
        private Automaton _nfa;

        public NFA(Automaton nfa)
        {
            _nfa = nfa;
            _start_state = new State(_nfa);
            _nfa.AddStartState(_start_state);
        }

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
        public void post2nfa(Pattern pattern)
        {
            var tree = pattern.AstNode as SpecParserParser.PatternContext;
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
                    bool first = true;
                    for (int i = p.ChildCount - 2; i >= 0; --i)
                    {
                        // For Piggy, we're going to use a special edge
                        // to denote attribute or child node recognition
                        // because there is lookahead for the attribute
                        // or child node, not one symbol. In addition, once
                        // a child is found next, no attributes can occur.
                        Fragment f = fragmentStack.Pop();
                        //System.Console.Error.WriteLine(f.ToString());
                        //System.Console.Error.WriteLine(nfa.ToString());
                        if (i > 0 && first)
                        {
                            var foobar = last.StartState.Owner.SuccessorEdges(last.StartState);
                            var cc = foobar.Count();
                            var ff = foobar.FirstOrDefault();
                            if (cc == 1 &&
                                 (ff.IsCode ||
                                 ff.IsText))
                            {
                            }
                            else
                            {
                                // Add in ".*"
                                State s1 = new State(_nfa);
                                State s2 = new State(_nfa);
                                State s3 = new State(_nfa);
                                var e1 = new Edge(_nfa, s1, s2, null, Edge.EmptyAst);
                                var e2 = new Edge(_nfa, s2, s3, null, Edge.EmptyAst);
                                var e3 = new Edge(_nfa, s2, s2, null, Edge.EmptyAst, (int)Edge.EdgeModifiers.Any);
                                var e4 = new Edge(_nfa, s3, last.StartState, null, Edge.EmptyAst);
                                last = new Fragment(s1, last.OutStates);
                            }
                            //first = false;
                        }
                        foreach (var o in f.OutStates)
                        {
                            var e5 = new Edge(_nfa, o, last.StartState, null, Edge.EmptyAst);
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
                        State s1 = new State(_nfa);
                        State s2 = new State(_nfa);
                        var e = new Edge(_nfa, s1, s2, null, new List<IParseTree>() { t });
                        var f = new Fragment(s1, s2);
                        fragmentStack.Push(f);
                    }
                }
                else if (p as SpecParserParser.Id_or_star_or_emptyContext != null)
                {
                    var c = p.GetChild(0);
                    State s1 = new State(_nfa);
                    State s2 = new State(_nfa);
                    var e = new Edge(_nfa, s1, s2, null, new List<IParseTree>() { c });
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.MoreContext != null) { }
                else if (p as SpecParserParser.TextContext != null)
                {
                    State s1 = new State(_nfa);
                    State s2 = new State(_nfa);
                    var e = new Edge(_nfa, s1, s2, null, new List<IParseTree>() { p }, (int)Edge.EdgeModifiers.Text);
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.CodeContext != null)
                {
                    State s1 = new State(_nfa);
                    State s2 = new State(_nfa);
                    var e = new Edge(_nfa, s1, s2, null, new List<IParseTree>() { p }, (int)Edge.EdgeModifiers.Code);
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.Group_rexpContext != null) { }
                else if (p as SpecParserParser.Star_rexpContext != null)
                {
                    Fragment previous = fragmentStack.Pop();
                    State s1 = new State(_nfa);
                    var e1 = new Edge(_nfa, s1, previous.StartState, null, Edge.EmptyAst);
                    foreach (var s in previous.OutStates)
                    {
                        var e2 = new Edge(_nfa, s, s1, null, Edge.EmptyAst);
                    }
                    var f = new Fragment(s1, s1);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.Plus_rexpContext != null)
                {
                    Fragment previous = fragmentStack.Pop();
                    State s1 = new State(_nfa);
                    State s2 = new State(_nfa);
                    var e1 = new Edge(_nfa, s1, s2, null, Edge.EmptyAst);
                    var e2 = new Edge(_nfa, s2, previous.StartState, null, Edge.EmptyAst);
                    foreach (var s in previous.OutStates)
                    {
                        var e3 = new Edge(_nfa, s, s2, null, Edge.EmptyAst);
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
                        State s1 = new State(_nfa);
                        State s2 = new State(_nfa);
                        var e = new Edge(_nfa, s1, s2, null, new List<IParseTree>() { t }, (int)Edge.EdgeModifiers.Not);
                        var f = new Fragment(s1, s2);
                        fragmentStack.Push(f);
                    }
                    else
                    {
                        TerminalNodeImpl t = c as TerminalNodeImpl;
                        var s = t.Symbol;
                        var s_type = s.Type;
                        State s1 = new State(_nfa);
                        State s2 = new State(_nfa);
                        State s3 = new State(_nfa);
                        State s4 = new State(_nfa);
                        var e1 = new Edge(_nfa, s1, s2, null, new List<IParseTree>() { t });
                        t = p.GetChild(1) as TerminalNodeImpl;
                        var e2 = new Edge(_nfa, s2, s3, null, new List<IParseTree>() { t });
                        t = p.GetChild(2) as TerminalNodeImpl;
                        var e3 = new Edge(_nfa, s3, s4, null, new List<IParseTree>() { t });
                        var f = new Fragment(s1, s4);
                        fragmentStack.Push(f);
                    }
                }
                else if (p as SpecParserParser.RexpContext != null)
                {
                    for (int i = 2; i < p.ChildCount; i += 2)
                    {
                        State s = new State(_nfa);
                        Fragment s2 = fragmentStack.Pop();
                        Fragment s1 = fragmentStack.Pop();
                        var e1 = new Edge(_nfa, s, s1.StartState, null, Edge.EmptyAst);
                        var e2 = new Edge(_nfa, s, s2.StartState, null, Edge.EmptyAst);
                        State s3 = new State(_nfa);
                        foreach (var o in s1.OutStates)
                        {
                            var e3 = new Edge(_nfa, o, s3, null, Edge.EmptyAst);
                        }
                        foreach (var o in s2.OutStates)
                        {
                            var e3 = new Edge(_nfa, o, s3, null, Edge.EmptyAst);
                        }
                        var f = new Fragment(s, s3);
                        fragmentStack.Push(f);
                    }
                }
            }
            completeNfa = fragmentStack.Pop();
            if (fragmentStack.Count > 0)
                throw new System.Exception("Fragment stack not empty.");
            foreach (var s in completeNfa.OutStates) _nfa.AddFinalState(s);
            var eek = new Edge(_nfa, _start_state, completeNfa.StartState,
                null, Edge.EmptyAst);
        }
    }
}
