using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Tree;
using Runtime;

namespace Engine
{
    /**
     * NFA via Thompson's Construction.
     */
    public class ThompsonsConstruction
    {
        private readonly Automaton _nfa;
        private readonly State _start_state;
        private int _order = 0;

        public Automaton NFA
        {
            get { return _nfa; }
        }

        public ThompsonsConstruction()
        {
            _nfa = new Automaton();
            _start_state = new State(_nfa);
            _nfa.AddStartState(_start_state);
        }

        /**
         * Computes and returns the post-order nodes of a tree.
         */
        private static List<IParseTree> ComputePostOrder(IParseTree tree)
        {
            var stack = new Stack<IParseTree>();
            var visited = new HashSet<IParseTree>();
            stack.Push(tree);
            var post_order = new List<IParseTree>();
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (visited.Contains(v))
                {
                    post_order.Add(v);
                }
                else
                {
                    stack.Push(v);
                    visited.Add(v);
                    for (var i = v.ChildCount - 1; i >= 0; --i)
                    {
                        var c = v.GetChild(i);
                        stack.Push(c);
                    }
                }
            }

            return post_order;
        }

        /**
         * Generate a NFA from the given pattern using Thompson's Construction and
         * add it to the current overall NFA.
         */
        public void post2nfa(Pattern pattern)
        {
            var tree = pattern.AstNode as SpecParserParser.PatternContext;
            var post_order = ComputePostOrder(tree);
            var fragmentStack = new Stack<Fragment>();
            var completeNfa = new Fragment();
            foreach (var p in post_order)
            {
                var p_text = p.GetText();
                var p_type = p.GetType();

                // simple_basic, kleen_star_basic, and continued_basic are very special:
                // add in ".*" between each item.
                if (p as SpecParserParser.Simple_basicContext != null
                    || p as SpecParserParser.Kleene_star_basicContext != null
                    )
                {
                    var last = fragmentStack.Pop();
                    var first = true;
                    var c = p.GetChild(0);
                    bool not = (c.GetText() == "!");
                    for (var i = p.ChildCount - (not ? 3:2); i >= 0; --i)
                    {
                        // For Piggy, we're going to use a special edge
                        // to denote attribute or child node recognition
                        // because there is lookahead for the attribute
                        // or child node, not one symbol. In addition, once
                        // a child is found next, no attributes can occur.
                        var f = fragmentStack.Pop();
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
                                var s1 = new State(_nfa);
                                var s2 = new State(_nfa);
                                var s3 = new State(_nfa);
                                var e1 = new Edge(_nfa, s1, s2, Edge.EmptyAst);
                                var e2 = new Edge(_nfa, s2, s3, Edge.EmptyAst);
                                var e3 = new Edge(_nfa, s2, s2, Edge.EmptyAst, (int) Edge.EdgeModifiersEnum.Any);
                                var e4 = new Edge(_nfa, s3, last.StartState, Edge.EmptyAst);
                                last = new Fragment(s1, last.OutStates);
                            }
                        }
                        foreach (var o in f.OutStates)
                        {
                            var e5 = new Edge(_nfa, o, last.StartState, Edge.EmptyAst);
                        }
                        last = new Fragment(f.StartState, last.OutStates);
                    }
                    fragmentStack.Push(last);
                }
                else if (p as SpecParserParser.BasicContext != null)
                {
                }
                else if (p as SpecParserParser.PatternContext != null)
                {
                }
                else if (p as TerminalNodeImpl != null)
                {
                    var t = p as TerminalNodeImpl;
                    var s = t.Symbol;
                    var s_type = s.Type;
                    if (s.Type == SpecParserParser.OPEN_PAREN ||
                        s.Type == SpecParserParser.OPEN_KLEENE_STAR_PAREN ||
                        s.Type == SpecParserParser.CLOSE_PAREN ||
                        s.Type == SpecParserParser.CLOSE_KLEENE_STAR_PAREN)
                    {
                        var s1 = new State(_nfa);
                        var s2 = new State(_nfa);
                        var e = new Edge(_nfa, s1, s2, new List<IParseTree> {t});
                        var f = new Fragment(s1, s2);
                        fragmentStack.Push(f);
                    }
                }
                else if (p as SpecParserParser.Id_or_star_or_emptyContext != null)
                {
                    var c = p.GetChild(0);
                    var s1 = new State(_nfa);
                    var s2 = new State(_nfa);
                    var e = new Edge(_nfa, s1, s2, new List<IParseTree> {c});
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.MoreContext != null)
                {
                }
                else if (p as SpecParserParser.TextContext != null)
                {
                    var s1 = new State(_nfa);
                    var s2 = new State(_nfa);
                    var e = new Edge(_nfa, s1, s2, new List<IParseTree> {p}, (int) Edge.EdgeModifiersEnum.Text);
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.CodeContext != null)
                {
                    var s1 = new State(_nfa);
                    var s2 = new State(_nfa);
                    var e = new Edge(_nfa, s1, s2, new List<IParseTree> {p}, (int) Edge.EdgeModifiersEnum.Code);
                    var f = new Fragment(s1, s2);
                    fragmentStack.Push(f);
                }
                else if (p as SpecParserParser.Group_rexpContext != null)
                {
                }
                else if (p as SpecParserParser.Star_rexpContext != null)
                {
                    var previous = fragmentStack.Pop();

                    {
                        // Add in ".*" before "previous"
                        var s1 = new State(_nfa);
                        var s2 = new State(_nfa);
                        var s3 = new State(_nfa);
                        var e1 = new Edge(_nfa, s1, s2, Edge.EmptyAst);
                        var e2 = new Edge(_nfa, s2, s3, Edge.EmptyAst);
                        var e3 = new Edge(_nfa, s2, s2, Edge.EmptyAst, (int)Edge.EdgeModifiersEnum.Any);
                        var e4 = new Edge(_nfa, s3, previous.StartState, Edge.EmptyAst);
                        previous = new Fragment(s1, previous.OutStates);
                    }

                    {
                        // Add in state s1 before previous.
                        var s1 = new State(_nfa);
                        var e1 = new Edge(_nfa, s1, previous.StartState, Edge.EmptyAst);
                        // Add in back edges to s1.
                        foreach (var s in previous.OutStates)
                        {
                            var e2 = new Edge(_nfa, s, s1, Edge.EmptyAst);
                        }
                        var f = new Fragment(s1, s1);
                        fragmentStack.Push(f);
                    }
                }
                else if (p as SpecParserParser.Plus_rexpContext != null)
                {
                    var previous = fragmentStack.Pop();

                    {
                        // Add in ".*" before "previous"
                        var s1 = new State(_nfa);
                        var s2 = new State(_nfa);
                        var s3 = new State(_nfa);
                        var e1 = new Edge(_nfa, s1, s2, Edge.EmptyAst);
                        var e2 = new Edge(_nfa, s2, s3, Edge.EmptyAst);
                        var e3 = new Edge(_nfa, s2, s2, Edge.EmptyAst, (int)Edge.EdgeModifiersEnum.Any);
                        var e4 = new Edge(_nfa, s3, previous.StartState, Edge.EmptyAst);
                        previous = new Fragment(s1, previous.OutStates);
                    }

                    {
                        // Add state after previous outstates.
                        var s1 = new State(_nfa);
                        foreach (var s in previous.OutStates)
                        {
                            var e3 = new Edge(_nfa, s, s1, Edge.EmptyAst);
                        }
                        // Add edge from s1 back edge to previous.
                        var e4 = new Edge(_nfa, s1, previous.StartState, Edge.EmptyAst);
                        // Finish up with fragment.
                        var f = new Fragment(previous.StartState, s1);
                        fragmentStack.Push(f);
                    }
                }
                else if (p as SpecParserParser.AttrContext != null)
                {
                    var c = p.GetChild(0);
                    if (c.GetText() == "!")
                    {
                        var c2 = p.GetChild(1);
                        var t = c2 as TerminalNodeImpl;
                        var s = t.Symbol;
                        var s_type = s.Type;
                        var s1 = new State(_nfa);
                        var s2 = new State(_nfa);
                        var e = new Edge(_nfa, s1, s2, new List<IParseTree> {t}, (int) Edge.EdgeModifiersEnum.Not);
                        var f = new Fragment(s1, s2);
                        fragmentStack.Push(f);
                    }
                    else
                    {
                        var t = c as TerminalNodeImpl;
                        var s = t.Symbol;
                        var s_type = s.Type;
                        var s1 = new State(_nfa);
                        var s2 = new State(_nfa);
                        var s3 = new State(_nfa);
                        var s4 = new State(_nfa);
                        var e1 = new Edge(_nfa, s1, s2, new List<IParseTree> {t});
                        t = p.GetChild(1) as TerminalNodeImpl;
                        var e2 = new Edge(_nfa, s2, s3, new List<IParseTree> {t});
                        t = p.GetChild(2) as TerminalNodeImpl;
                        var e3 = new Edge(_nfa, s3, s4, new List<IParseTree> {t});
                        var f = new Fragment(s1, s4);
                        fragmentStack.Push(f);
                    }
                }
                else if (p as SpecParserParser.RexpContext != null)
                {
                    for (var i = 2; i < p.ChildCount; i += 2)
                    {
                        var s = new State(_nfa);
                        var s2 = fragmentStack.Pop();
                        {
                            // Add in ".*"
                            var sa = new State(_nfa);
                            var sb = new State(_nfa);
                            var sc = new State(_nfa);
                            var ea = new Edge(_nfa, sa, sb, Edge.EmptyAst);
                            var eb = new Edge(_nfa, sb, sc, Edge.EmptyAst);
                            var e3 = new Edge(_nfa, sb, sb, Edge.EmptyAst, (int)Edge.EdgeModifiersEnum.Any);
                            var e4 = new Edge(_nfa, sc, s2.StartState, Edge.EmptyAst);
                            s2 = new Fragment(sa, s2.OutStates);
                        }
                        var s1 = fragmentStack.Pop();
                        {
                            // Add in ".*"
                            var sa = new State(_nfa);
                            var sb = new State(_nfa);
                            var sc = new State(_nfa);
                            var ea = new Edge(_nfa, sa, sb, Edge.EmptyAst);
                            var eb = new Edge(_nfa, sb, sc, Edge.EmptyAst);
                            var e3 = new Edge(_nfa, sb, sb, Edge.EmptyAst, (int)Edge.EdgeModifiersEnum.Any);
                            var e4 = new Edge(_nfa, sc, s1.StartState, Edge.EmptyAst);
                            s1 = new Fragment(sa, s1.OutStates);
                        }
                        var e1 = new Edge(_nfa, s, s1.StartState, Edge.EmptyAst);
                        var e2 = new Edge(_nfa, s, s2.StartState, Edge.EmptyAst);
                        var s3 = new State(_nfa);
                        foreach (var o in s1.OutStates)
                        {
                            var e3 = new Edge(_nfa, o, s3, Edge.EmptyAst);
                        }
                        foreach (var o in s2.OutStates)
                        {
                            var e3 = new Edge(_nfa, o, s3, Edge.EmptyAst);
                        }
                        var f = new Fragment(s, s3);
                        fragmentStack.Push(f);
                    }
                }
            }

            completeNfa = fragmentStack.Pop();
            if (fragmentStack.Count > 0)
                throw new Exception("Fragment stack not empty.");
            foreach (var s in completeNfa.OutStates) _nfa.AddFinalState(s);

            // Add in the NFA for this pattern into overall NFA.
            var eek = new Edge(_nfa, _start_state, completeNfa.StartState, Edge.EmptyAst);
        }
    }
}