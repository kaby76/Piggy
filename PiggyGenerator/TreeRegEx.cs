using System.Linq;

namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;
    using Antlr4.Runtime;
    using PiggyRuntime;
    using System.Collections.Generic;
    using System;
    using System.Text.RegularExpressions;

    public class TreeRegEx
    {
        public IParseTree _ast;
        public Type _current_type;
        public object _instance;
        public CommonTokenStream _common_token_stream;
        public HashSet<IParseTree> _matches = new HashSet<IParseTree>();
        public HashSet<IParseTree> _top_level_matches = new HashSet<IParseTree>();
        public Intercept<IParseTree, Path> _matches_path_start = new Intercept<IParseTree, Path>();
        public List<Pass> _passes;
        public Piggy _piggy;

        public TreeRegEx(Piggy piggy, List<Pass> passes_with_common_name, object instance)
        {
            _piggy = piggy;
            _ast = _piggy._ast.GetChild(0);
            _passes = passes_with_common_name;
            _instance = instance;
            _common_token_stream = _piggy._common_token_stream;
        }

        public void Match()
        {
            foreach (var pass in this._passes)
            {
                _current_type = pass.Owner.Type;
                var nfa = new Automaton();
                var n = new NFA(nfa);
                foreach (Pattern pattern in pass.Patterns) n.post2nfa(pattern);
                System.Console.Error.WriteLine(nfa);
                var nfa_to_dfa = new NFAToDFA();
                var dfa = nfa_to_dfa.ConvertToDFA(nfa);
                System.Console.Error.WriteLine(dfa);

                // Perform naive matching for each node.
                foreach (var input in this._ast.Preorder())
                {
                    if (!(input as AstParserParser.NodeContext != null ||
                          input as AstParserParser.AttrContext != null))
                        continue;

                    bool has_previous_match = _matches.Contains(input);
                    bool do_matching = (!has_previous_match);
                    if (has_previous_match)
                        continue;

                    var currentStateList = new List<State>();
                    var currentPathList = new List<Path>();
                    var nextPathList = new List<Path>();
                    var nextStateList = new List<State>();
                    var nfa_match = new NfaMatch(this._piggy._code_blocks, this._instance, dfa);
                    var start = dfa.StartStates.FirstOrDefault().Id;
                    var st = dfa.AllStates().Where(s => s.Id == start).FirstOrDefault();
                    nfa_match.AddStateAndClosure(currentStateList, st);
                    var matched = nfa_match.FindMatches(currentPathList, currentStateList, ref nextPathList, ref nextStateList, input);
                    if (matched)
                    {
                        // If this node matched, then mark entire subtree as matched.
                        var stack = new Stack<IParseTree>();
                        stack.Push(input);
                        while (stack.Count > 0)
                        {
                            var v = stack.Pop();
                            _matches.Add(v);
                            for (int i = v.ChildCount - 1; i >= 0; --i)
                            {
                                var c = v.GetChild(i);
                                stack.Push(c);
                            }
                        }
                        _top_level_matches.Add(input);
                        foreach (Path p in nextPathList)
                        {
                            _matches_path_start.MyAdd(input, p);
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
