namespace PiggyGenerator
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
        public HashSet<IParseTree> _top_level_matches = new HashSet<IParseTree>();
        public Intercept<IParseTree, Path> _top_level_paths = new Intercept<IParseTree, Path>();
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
            var stack = new Stack<IParseTree>();
            stack.Push(_ast);

            // Combine all patterns of a pass into one NFA.
            foreach (var pass in this._passes)
            {
                var nfa = new Automaton();
                foreach (Pattern pattern in pass.Patterns)
                {
                    SpecParserParser.PatternContext t = pattern.AstNode as SpecParserParser.PatternContext;
                    NFA.post2nfa(nfa, t);
                }

                _current_type = pass.Owner.Type;
                var nfa_to_dfa = new NFAToDFA();
                var dfa = nfa_to_dfa.ConvertToDFA(nfa);
                System.Console.Error.WriteLine(dfa);
                foreach (var ast_node in _pre_order)
                {
                    var nfa_match = new NfaMatch(this);
                    // Try matching at vertex, if the node hasn't been already matched.
                    _matches.TryGetValue(ast_node, out List<IParseTree> val);
                    bool do_matching = val == null || !val.Where(xx => IsPatternKleene(xx) || IsPatternSimple(xx)).Any();
                    var matched = do_matching && nfa_match.IsMatch(dfa, ast_node);
                    if (matched)
                    {
                        foreach (Path p in nfa_match.MatchingPaths)
                        {
                            _top_level_paths.MyAdd(ast_node, p);
                            foreach (var ee in p)
                            {
                                var e = ee.LastEdge;
                                if (e._c != null)
                                {
                                    var pat = e._c;
                                    foreach (IParseTree ast in new NfaMatch.EnumerableIParseTree(ast_node))
                                    {
                                        if (ast as TerminalNodeImpl == null)
                                            continue;
                                        _matches.MyAdd(ast, pat);
                                    }
                                    _top_level_matches.Add(ast_node);
                                }
                            }
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

        public string ReplaceMacro(IParseTree p)
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
    }
}
