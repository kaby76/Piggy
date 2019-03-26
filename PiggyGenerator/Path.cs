﻿namespace PiggyGenerator
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Antlr4.Runtime.Tree;
    using System.Linq;
    using System;

    public class Path : IEnumerable<Path>
    {
        readonly Path _next;
        readonly Edge _transition;
        readonly IParseTree _input;
        readonly int change;

        public Path(Edge e, IParseTree input)
        {
            _next = null;
            _transition = e;
            _input = input;
            change = 0;
        }

        public Path(Path n, Edge e, IParseTree input)
        {
            _next = n;
            if (n == null) throw new Exception();
            _transition = e;
            _input = input;
            if (e.IsAny && input.GetText() == "(")
                change = n.change + 1;
            else if (e.IsAny && input.GetText() == ")")
                change = n.change - 1;
            else change = n.change;
        }

        public Path Next
        {
            get { return _next; }
        }

        public Edge LastEdge
        {
            get { return _transition; }
        }

        public IParseTree Ast
        {
            get { return _input; }
        }

        public int Change { get { return change; } }

        private IEnumerator<Path> Doit()
        {
            // Follow Next link to get path in reverse.
            // Then, iterate over each Path object to get the total
            // path in a forward direction.
            Stack<Path> stack = new Stack<Path>();
            var p = this;
            while (p != null)
            {
                stack.Push(p);
                p = p.Next;
            }
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                yield return v;
            }
        }

        public IEnumerator<Path> GetEnumerator()
        {
            return Doit();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Doit();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Stack<Path> stack = new Stack<Path>();
            var p = this;
            while (p != null)
            {
                stack.Push(p);
                p = p.Next;
            }
            bool first = true;
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (first)
                {
                    sb.Append(v._transition._from + " ");
                    first = false;
                }
                sb.Append("-> " + v._transition._to);
            }
            return sb.ToString();
        }
    }
}
