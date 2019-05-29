using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PiggyRuntime;

namespace PiggyGenerator
{
    public class TreeRegEx
    {
        public IParseTree _ast;
        public CommonTokenStream _common_token_stream;
        public Type _current_type;
        public object _instance;
        public HashSet<IParseTree> _matches = new HashSet<IParseTree>();
        public Intercept<IParseTree, Path> _matches_path_start = new Intercept<IParseTree, Path>();
        public List<Pass> _passes;
        public Piggy _piggy;
        public HashSet<IParseTree> _top_level_matches = new HashSet<IParseTree>();

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
            foreach (var pass in _passes)
            {
                _current_type = pass.Owner.Type;
                if (Piggy._debug_information) Console.Error.WriteLine(pass.Owner.TemplateName + " " + pass.Name);
                var thompsons_construction = new ThompsonsConstruction();
                var nfa = thompsons_construction.NFA;
                foreach (var pattern in pass.Patterns) thompsons_construction.post2nfa(pattern);
                if (Piggy._debug_information) Console.Error.WriteLine(nfa);
                var nfa_optimizer = new NfaOptimizer();
                Automaton optimized_nfa = null;
                if (false)
                    optimized_nfa = nfa_optimizer.Optimize(nfa);
                else
                    optimized_nfa = nfa;
                if (Piggy._debug_information) Console.Error.WriteLine(optimized_nfa);

                // Perform naive matching for each node.
                foreach (var input in _ast.Preorder())
                {
                    if (!(input as AstParserParser.NodeContext != null ||
                          input as AstParserParser.AttrContext != null))
                        continue;

                    var has_previous_match = _matches.Contains(input);
                    var do_matching = !has_previous_match;
                    if (has_previous_match)
                        continue;

                    var currentStateList = new List<State>();
                    var currentPathList = new List<Path>();
                    var nextPathList = new List<Path>();
                    var nextStateList = new List<State>();
                    var nfa_match = new NfaMatch(_piggy._code_blocks, _instance, optimized_nfa);
                    var start = optimized_nfa.StartStates.FirstOrDefault().Id;
                    var st = optimized_nfa.AllStates().Where(s => s.Id == start).FirstOrDefault();
                    nfa_match.AddStateAndClosure(currentStateList, st);
                    if (Piggy._debug_information) System.Console.Error.WriteLine("Looking at " + input.GetText().Truncate(40));
                    var matched = nfa_match.FindMatches(currentPathList, currentStateList, ref nextPathList,
                        ref nextStateList, input);
                    if (matched)
                    {
                        // If this node matched, then mark entire subtree as matched.
                        var stack = new Stack<IParseTree>();
                        stack.Push(input);
                        while (stack.Count > 0)
                        {
                            var v = stack.Pop();
                            _matches.Add(v);
                            for (var i = v.ChildCount - 1; i >= 0; --i)
                            {
                                var c = v.GetChild(i);
                                stack.Push(c);
                            }
                        }

                        _top_level_matches.Add(input);
                        foreach (var p in nextPathList) _matches_path_start.MyAdd(input, p);
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
            if (context as TerminalNodeImpl != null) return context.GetText();
            var x = context as ParserRuleContext;
            if (x == null) return "UNKNOWN TYPE!";
            var c = x;
            var startToken = c.Start;
            var stopToken = c.Stop;
            var cs = startToken.InputStream;
            var startIndex = startToken.StartIndex;
            var stopIndex = stopToken.StopIndex;
            if (startIndex > stopIndex)
                startIndex = stopIndex;
            return cs.GetText(new Interval(startIndex, stopIndex));
        }
    }
}