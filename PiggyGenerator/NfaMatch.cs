using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Tree;
using PiggyRuntime;

namespace PiggyGenerator
{
    public class NfaMatch
    {
        private readonly Dictionary<IParseTree, MethodInfo> _code_blocks;
        private readonly object _instance;
        private Automaton nfa;

        public NfaMatch(Dictionary<IParseTree, MethodInfo> code_blocks,
            object instance,
            Automaton a)
        {
            nfa = a;
            _code_blocks = code_blocks;
            _instance = instance;
        }

        public string ReplaceMacro(IParseTree p)
        {
            // Try in order current type, then all other types.
            try
            {
                var res = _code_blocks[p].Invoke(_instance, new object[] { });
                return res as string;
            }
            catch (Exception e)
            {
            }

            throw new Exception("Cannot eval expression.");
        }

        private bool NonbacktrackingFindMatches(
            List<Path> currentPathList,
            List<State> currentStateList,
            ref List<Path> nextPathList,
            ref List<State> nextStateList,
            IParseTree input
        )
        {
            var listID = 0;

            var generation = new Dictionary<Edge, int>();

            Console.Error.WriteLine("IN------");
            Console.Error.WriteLine("FindMatches "
                                    + input.GetText().Substring(
                                        0,
                                        input.GetText().Length > 50 ? 50 : input.GetText().Length));
            foreach (var path in currentPathList)
                Console.Error.WriteLine(path.ToString());
            Console.Error.WriteLine("IN------");

            if (input.GetText().StartsWith("(EnumDecl"))
            { }

            // Variable "input" can be either one of two types:
            // AstParserParser.DeclContext
            // AstParserParser.AttrContext
            // Go through all children and match.
            var i = 0;
            for (; ; )
            {
                var oldlist = currentPathList;
                var oldStateList = currentStateList;
                if (i >= input.ChildCount)
                {
                    nextPathList = currentPathList;
                    nextStateList = currentStateList;
                    break;
                }

                var c = input.GetChild(i);
                var t = c.GetText();

                if (t.StartsWith("(EnumConstantDecl"))
                { }

                i++;

                listID = Step(c, currentStateList, nextStateList, currentPathList, nextPathList, listID);
                if (!nextPathList.Any())
                    break;
                currentPathList = nextPathList;
                currentStateList = nextStateList;
                nextPathList = new List<Path>();
                nextStateList = new List<State>();
            }

            // Compute results.
            var matches = 0;
            for (var j = 0; j < nextPathList.Count; j++)
            {
                var l = nextPathList[j];
                var e = l.LastEdge;
                var s = e.To;
                if (s.Owner.IsFinalState(s)) matches++;
            }

            foreach (var s in currentStateList)
                if (s.Owner.IsFinalState(s))
                {
                    matches++;
                    if (!nextStateList.Contains(s))
                        nextStateList.Add(s);
                }

            Console.Error.WriteLine("OUT------");
            Console.Error.WriteLine("FindMatches "
                                    + input.GetText().Substring(
                                        0,
                                        input.GetText().Length > 50 ? 50 : input.GetText().Length));
            foreach (var path in nextPathList)
                Console.Error.WriteLine(path.ToString());
            Console.Error.WriteLine("OUT------");
            return matches != 0;
        }

        private Dictionary<IParseTree, IParseTree> next_sibling;
        private Dictionary<IParseTree, IParseTree> next;

        private IParseTree NextSibling(IParseTree current)
        {
            return next_sibling[current];
        }

        private IParseTree Next(IParseTree current)
        {
            return next[current];
        }

        private Path BacktrackingFindMatches(
            State state,
            IParseTree current,
            out bool match
        )
        {
            // Variable "input" can be either one of three types:
            // AstParserParser.DeclContext
            // AstParserParser.AttrContext
            // terminal
            // Go through all children and match.

            System.Console.Error.WriteLine("BacktrackingFindMatches"
                + " state " + state
                + " input " + (current == null ? "''" : current.GetText().Truncate(30)));

            if (state.Owner.IsFinalState(state))
            {
                match = true;
                return null;
            }
            if (current == null)
            {
                foreach (var e in state.Owner.SuccessorEdges(state))
                {
                    var r = BacktrackingFindMatches(e, current, out match);
                    if (match)
                    {
                        if (r == null)
                            r = new Path(e, current);
                        System.Console.Error.WriteLine("returning " + r);
                        return r;
                    }
                }
            }
            else if (current as AstParserParser.NodeContext != null ||
                     current as AstParserParser.AttrContext != null)
            {
                foreach (var e in state.Owner.SuccessorEdges(state))
                {
                    var r = BacktrackingFindMatches(e, current, out match);
                    if (match)
                    {
                        if (r == null)
                            r = new Path(e, current);
                        System.Console.Error.WriteLine("returning " + r);
                        return r;
                    }
                }
            }
            else
            {
                // Terminal.
                foreach (var e in state.Owner.SuccessorEdges(state))
                {
                    var r = BacktrackingFindMatches(e, current, out match);
                    if (match)
                    {
                        if (r == null)
                            r = new Path(e, current);
                        System.Console.Error.WriteLine("returning " + r);
                        return r;
                    }
                }
            }
            System.Console.Error.WriteLine("fail");
            match = false;
            return null;
        }

        private Path BacktrackingFindMatches(
            Edge e,
            IParseTree current,
            out bool match
        )
        {

            // Variable "input" can be either one of three types:
            // AstParserParser.DeclContext
            // AstParserParser.AttrContext
            // terminal
            // Go through all children and match.
            var state = e.To;

            System.Console.Error.WriteLine(
                "BacktrackingFindMatches"
                + " state " + state
                + " input " + (current == null ? "''" : current.GetText().Truncate(30) + " " + current.GetType()));

            if (current == null)
            {
                if (state.Owner.IsFinalState(state))
                {
                    var p = new Path(e, current);
                    match = true;
                    return p;
                }
                if (e.IsEmpty || e.IsCode || e.IsText)
                {
                    var r = BacktrackingFindMatches(e.To, current, out match);
                    if (match)
                    {
                        if (r == null)
                            r = new Path(e, current);
                        else
                        { var p = new Path(e, current, r); }
                        System.Console.Error.WriteLine("returning " + r);
                        return r;
                    }
                }
            }
            else if (current as AstParserParser.NodeContext != null ||
                     current as AstParserParser.AttrContext != null)
            {
                if (e.IsEmpty || e.IsCode || e.IsText)
                {
                    var r = BacktrackingFindMatches(e.To, current, out match);
                    if (match)
                    {
                        if (r == null)
                            r = new Path(e, current);
                        else
                        { var p = new Path(e, current, r); }
                        System.Console.Error.WriteLine("returning " + r);
                        return r;
                    }
                }
                else if (e.IsNot)
                {
                    // To match this, go up to parent and scan all children
                    IParseTree parent = current.Parent;
                    bool found = false;
                    foreach (var child in parent.ChildrenForward())
                    {
                        if (child as AstParserParser.AttrContext != null)
                        {
                            if (child.GetChild(0).GetText() == e.Input)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        IParseTree n = current;
                        var r = BacktrackingFindMatches(e.To, n, out match);
                        if (match)
                        {
                            if (r == null)
                                r = new Path(e, current);
                            else
                            { var p = new Path(e, current, r); }
                            System.Console.Error.WriteLine("returning " + r);
                            return r;
                        }
                    }
                }
                else if (e.IsAny)
                {
                    IParseTree n = NextSibling(current);
                    var r = BacktrackingFindMatches(e.To, n, out match);
                    if (match)
                    {
                        if (r == null)
                            r = new Path(e, current);
                        else
                        { var p = new Path(e, current, r); }
                        System.Console.Error.WriteLine("returning " + r);
                        return r;
                    }
                }
                else
                {
                    // Delve down into child and step through
                    // all siblings.
                    IParseTree n = current.GetChild(0);
                    var r = BacktrackingFindMatches(e, n, out match);
                    if (match)
                    {
                        System.Console.Error.WriteLine("returning " + r);
                        return r;
                    }
                }
            }
            else
            {
                // Terminal.
                if (e.IsEmpty || e.IsCode || e.IsText)
                {
                    var r = BacktrackingFindMatches(e.To, current, out match);
                    if (match)
                    {
                        if (r == null)
                            r = new Path(e, current);
                        else
                        { var p = new Path(e, current, r);}
                        System.Console.Error.WriteLine("returning " + r);
                        return r;
                    }
                }
                else if (e.IsAny)
                {
                }
                else if (e.IsNot)
                {
                    // To match this, go up to parent and scan all children
                    IParseTree parent = current.Parent;
                    bool found = false;
                    foreach (var child in parent.ChildrenForward())
                    {
                        if (child as AstParserParser.AttrContext != null)
                        {
                            if (child.GetChild(0).GetText() == e.Input)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        IParseTree n = current;
                        var r = BacktrackingFindMatches(e.To, n, out match);
                        if (match)
                        {
                            if (r == null)
                                r = new Path(e, current);
                            else
                            { var p = new Path(e, current, r); }
                            System.Console.Error.WriteLine("returning " + r);
                            return r;
                        }
                    }
                }
                else if (e.Input == current.GetText())
                {
                    IParseTree n = Next(current);
                    var r = BacktrackingFindMatches(e.To, n, out match);
                    if (match)
                    {
                        if (r == null)
                            r = new Path(e, current);
                        else
                        { var p = new Path(e, current, r); }
                        System.Console.Error.WriteLine("returning " + r);
                        return r;
                    }
                }
                else if (e.Input == "<" && "(" == current.GetText())
                {
                    IParseTree n = Next(current);
                    var r = BacktrackingFindMatches(e.To, n, out match);
                    if (match)
                    {
                        if (r == null)
                            r = new Path(e, current);
                        else
                        { var p = new Path(e, current, r); }
                        System.Console.Error.WriteLine("returning " + r);
                        return r;
                    }
                }
                else if (e.Input == ">" && ")" == current.GetText())
                {
                    IParseTree n = Next(current);
                    var r = BacktrackingFindMatches(e.To, n, out match);
                    if (match)
                    {
                        if (r == null)
                            r = new Path(e, current);
                        else
                        { var p = new Path(e, current, r); }
                        System.Console.Error.WriteLine("returning " + r);
                        return r;
                    }
                }
                else if (e.Input == "*")
                {
                    IParseTree n = Next(current);
                    var r = BacktrackingFindMatches(e.To, n, out match);
                    if (match)
                    {
                        if (r == null)
                            r = new Path(e, current);
                        else
                        { var p = new Path(e, current, r); }
                        System.Console.Error.WriteLine("returning " + r);
                        return r;
                    }
                }
                else if (e.Input.StartsWith("$\""))
                {
                    var pattern = e.Input.Substring(2);
                    pattern = pattern.Substring(0, pattern.Length - 1);
                    try
                    {
                        var ch = e.Input;
                        if (e.AstList.Count() > 1)
                            ;
                        //throw new Exception("Cannot compute interpolated pattern because there are multiple paths through the DFA with this edge.");
                        var attr = e.AstList.First();
                        pattern = ReplaceMacro(attr);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot perform substitution in pattern with string.");
                        Console.WriteLine("Pattern " + pattern);
                        Console.WriteLine(ex.Message);
                        throw ex;
                    }

                    pattern = pattern.Replace("\\", "\\\\");
                    var re = new Regex(pattern);
                    var tvaltext = current.GetText();
                    tvaltext = tvaltext.Substring(1);
                    tvaltext = tvaltext.Substring(0, tvaltext.Length - 1);
                    var matched = re.Match(tvaltext);
                    var result = matched.Success;
                    if (result)
                    {
                        IParseTree n = Next(current);
                        var r = BacktrackingFindMatches(e.To, n, out match);
                        if (match)
                        {
                            if (r == null)
                                r = new Path(e, current);
                            else
                            { var p = new Path(e, current, r); }
                            System.Console.Error.WriteLine("returning " + r);
                            return r;
                        }
                    }
                }
            }
            System.Console.Error.WriteLine("fail");
            match = false;
            return null;
        }

        public bool FindMatches(
            List<Path> currentPathList,
            List<State> currentStateList,
            ref List<Path> nextPathList,
            ref List<State> nextStateList,
            IParseTree input,
            bool use_backtracking = true)
        {
            if (!(input as AstParserParser.NodeContext != null || input as AstParserParser.AttrContext != null))
                throw new Exception();

            if (use_backtracking)
            {
                next = new Dictionary<IParseTree, IParseTree>();
                next_sibling = new Dictionary<IParseTree, IParseTree>();
                IParseTree last = null;
                var rev = input.Preorder().ToList();
                rev.Reverse();
                foreach (var v in rev)
                {
                    next[v] = last;
                    if (v as AstParserParser.NodeContext != null ||
                        v as AstParserParser.AttrContext != null)
                    {
                        IParseTree last_child = null;
                        foreach (var child in v.ChildrenReverse())
                        {
                            next_sibling[child] = last_child;
                            last_child = child;
                        }
                    }
                    last = v;
                }
                var x = BacktrackingFindMatches(currentStateList.First(), input, out bool match);
                if (match)
                {
                    nextPathList.Add(x);
                    return true;
                }
                else
                    return false;
            }
            else
                return NonbacktrackingFindMatches(currentPathList, currentStateList, ref nextPathList, ref nextStateList, input);
        }

        public void AddStateAndClosure(List<State> list, State s)
        {
            if (list.Contains(s)) return;
            list.Add(s);
            foreach (var e in s.Owner.SuccessorEdges(s))
                if (e.IsEmpty)
                    AddStateAndClosure(list, e.To);
        }

        private void CheckPath(Path path)
        {
            return;
            State p = null;
            foreach (var ee in path)
            {
                var e = ee.LastEdge;
                if (p == null) p = e.To;

                if (p != e.From)
                    throw new Exception();
                p = e.To;
            }
        }

        private int Step(
            IParseTree input,
            List<State> currentStateList,
            List<State> nextStateList,
            List<Path> currentPathList,
            List<Path> nextPathList,
            int listID)
        {
            Console.Error.WriteLine("IN------");
            Console.Error.WriteLine("Step "
                                    + input.GetText().Substring(
                                        0,
                                        input.GetText().Length > 50 ? 50 : input.GetText().Length));
            foreach (var path in currentPathList)
                Console.Error.WriteLine(path.ToString());
            Console.Error.WriteLine("IN------");


            var t = input.GetText();
            var is_attr_or_node = input as AstParserParser.NodeContext != null ||
                                  input as AstParserParser.AttrContext != null;
            listID++;
            var ty = input.GetType();
            if (currentPathList == null || currentPathList.Count == 0)
            {
                for (var i = 0; i < currentStateList.Count; i++)
                {
                    var s = currentStateList[i];
                    foreach (var e in s.Owner.SuccessorEdges(s))
                        if (e.Input == Edge.EmptyString)
                            AppendEdgeToPathSet(null, null, nextPathList, e, listID);
                        else if (e.Input == input.GetText())
                            AppendEdgeToPathSet(input, null, nextPathList, e, listID);
                        else if (e.Input == "<" && "(" == input.GetText())
                            AppendEdgeToPathSet(input, null, nextPathList, e, listID);
                        else if (e.Input == ">" && ")" == input.GetText())
                            AppendEdgeToPathSet(input, null, nextPathList, e, listID);
                        else if (e.IsAny) AppendEdgeToPathSet(input, null, nextPathList, e, listID);
                }
            }
            else
            {
                if (is_attr_or_node)
                {
                    for (var i = 0; i < currentPathList.Count; i++)
                    {
                        var p = currentPathList[i];
                        CheckPath(p);
                        var l = p.LastEdge;
                        var s = l.To;
                        foreach (var e in s.Owner.SuccessorEdges(s))
                        {
                            if (e.IsAny)
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            else if (e.IsNot && input as AstParserParser.NodeContext != null)
                            {
                                // Look back in input and verify no attr of this type in
                                // back siblings, and we are on the first AstParserParser.NodeContext node.
                                var parent = input.Parent;
                                IParseTree child = null;
                                bool saw = false;
                                for (int k = 2; k < parent.ChildCount; ++k)
                                {
                                    child = parent.GetChild(k);
                                    if (child as AstParserParser.AttrContext != null)
                                    {
                                        if (child.GetChild(0).GetText() == e.Input) saw = true;
                                    }
                                    if (child as AstParserParser.NodeContext != null) break;
                                }
                                if (child == input && ! saw)
                                {
                                    AppendEdgeToPathSet(null, p, currentPathList, e, listID);
                                }
                            }
                        }
                    }

                    var cpl = currentPathList.ToList();
                    var csl = currentStateList.ToList();
                    var npl = new List<Path>();
                    var nsl = new List<State>();
                    FindMatches(currentPathList, currentStateList, ref npl, ref nsl, input, false);
                    if (npl.Count > 40)
                    { }
                    foreach (var p in npl) nextPathList.Add(p);
                    foreach (var s in nsl) nextStateList.Add(s);
                }
                else
                {
                    for (var i = 0; i < currentPathList.Count; i++)
                    {
                        var p = currentPathList[i];
                        CheckPath(p);
                        var l = p.LastEdge;
                        var s = l.To;
                        foreach (var e in s.Owner.SuccessorEdges(s))
                        {
                            if (e.IsAny)
                            {
                                // The "." transition only matches with attr or node
                            }
                            else if (e.IsNot)
                            {
                                // Only matches if the input is a
                                // AstParserParser.NodeContext.
                            }
                            else if (e.Input == input.GetText())
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e.Input == "<" && "(" == input.GetText())
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e.Input == ">" && ")" == input.GetText())
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e.Input == "*")
                            {
                                AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                            else if (e.Input.StartsWith("$\""))
                            {
                                var pattern = e.Input.Substring(2);
                                pattern = pattern.Substring(0, pattern.Length - 1);
                                try
                                {
                                    var ch = e.Input;
                                    if (e.AstList.Count() > 1)
                                        ;
                                    //throw new Exception("Cannot compute interpolated pattern because there are multiple paths through the DFA with this edge.");
                                    var attr = e.AstList.First();
                                    pattern = ReplaceMacro(attr);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Cannot perform substitution in pattern with string.");
                                    Console.WriteLine("Pattern " + pattern);
                                    Console.WriteLine(ex.Message);
                                    throw ex;
                                }

                                pattern = pattern.Replace("\\", "\\\\");
                                var re = new Regex(pattern);
                                var tvaltext = input.GetText();
                                tvaltext = tvaltext.Substring(1);
                                tvaltext = tvaltext.Substring(0, tvaltext.Length - 1);
                                var matched = re.Match(tvaltext);
                                var result = matched.Success;
                                if (result)
                                    AppendEdgeToPathSet(input, p, nextPathList, e, listID);
                            }
                        }
                    }
                }
            }

            foreach (var s in currentStateList)
                if (s.Owner.IsFinalState(s))
                    AddStateAndClosure(nextStateList, s);

            for (var i = 0; i < nextPathList.Count; i++)
            {
                var p = nextPathList[i];
                var l = p.LastEdge;
                var s = l.To;
                AddStateAndClosure(nextStateList, s);
            }

            Console.Error.WriteLine("OUT------");
            Console.Error.WriteLine("Step "
                                    + input.GetText().Substring(
                                        0,
                                        input.GetText().Length > 50 ? 50 : input.GetText().Length));
            foreach (var path in nextPathList)
                Console.Error.WriteLine(path.ToString());
            Console.Error.WriteLine("OUT------");
            return listID;
        }

        private void AppendEdgeToPathSet(IParseTree c, Path path, List<Path> list, Edge e, int listID)
        {
            var st = e.To;
            var sf = e.From;
            foreach (var l in list) CheckPath(l);

            if (path == null && !list.Any())
            {
                list.Add(new Path(e, c));
            }
            else
            {
                var p = new Path(path, e, c);
                CheckPath(p);
                list.Add(p);
            }

            var added = list.Last();
            // If s contains any edges over epsilon, then add them.
            foreach (var o in st.Owner.SuccessorEdges(st))
            {
                // Here we step along "epsilon" transitions to the next state as well.
                // In this parsing engine, code and text blocks function as epsilon
                // on input even though they aren't treated as so during construction of
                // the DFA.
                if (o.IsEmpty || o.IsCode || o.IsText)
                    AppendEdgeToPathSet(null, added, list, o, listID);
            }
        }
    }
}