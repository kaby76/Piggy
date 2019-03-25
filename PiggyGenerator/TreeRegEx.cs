namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;
    using Antlr4.Runtime;
    using PiggyRuntime;
    using System.Collections.Generic;
    using System;

    public class TreeRegEx
    {
        public IParseTree _ast;
        public Type _current_type;
        public object _instance;
        public CommonTokenStream _common_token_stream;
        public HashSet<IParseTree> _matches = new HashSet<IParseTree>();
        public HashSet<IParseTree> _top_level_matches = new HashSet<IParseTree>();
        public Intercept<IParseTree, Path> _matches_path_start = new Intercept<IParseTree, Path>();
        public Dictionary<IParseTree, IParseTree> _parent = new Dictionary<IParseTree, IParseTree>();
        public List<Pass> _passes;
        public Piggy _piggy;
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
        }

        public void Match()
        {
            var visited = new HashSet<IParseTree>();
            foreach (var pass in this._passes)
            {
                // Combine all patterns of a pass into one NFA,
                // then one, beautifully massive DFA.
                var nfa = new Automaton();
                foreach (Pattern pattern in pass.Patterns)
                {
                    NFA.post2nfa(nfa, pattern);
                }
                System.Console.Error.WriteLine(nfa);
                _current_type = pass.Owner.Type;
                var nfa_to_dfa = new NFAToDFA();
                var dfa = nfa_to_dfa.ConvertToDFA(nfa);
                System.Console.Error.WriteLine(dfa);

                // Perform naive matching for each node.
                foreach (var ast_node in _pre_order)
                {
                    var nfa_match = new NfaMatch(this._parent,
                        this._piggy._code_blocks, this._instance);
                    bool has_previous_match = _matches.Contains(ast_node);
                    bool do_matching = (!has_previous_match);
                    var matched = do_matching && nfa_match.FindMatches(dfa, ast_node);
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
                                if (!visited.Contains(c))
                                    stack.Push(c);
                            }
                        }
                        _top_level_matches.Add(ast_node);
                        foreach (Path p in nfa_match.MatchingPaths)
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
    }
}
