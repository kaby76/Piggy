namespace PiggyGenerator
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Antlr4.Runtime.Tree;
    using System.Linq;
    using System;

    public class Path : IEnumerable<Edge>
    {
        readonly Path _next;
        readonly Edge _last_edge;
        readonly IParseTree _c;

        public Path(Edge l, IParseTree c)
        {
            _next = null;
            _last_edge = l;
            _c = c;
        }

        public Path(Path n, Edge l, IParseTree c)
        {
            _next = n;
            if (n == null) throw new Exception();
            _last_edge = l;
            _c = c;
        }

        public Path Next
        {
            get { return _next; }
        }

        public Edge LastEdge
        {
            get { return _last_edge; }
        }

        private IEnumerator<Edge> Doit()
        {
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
                yield return v.LastEdge;
            }
        }

        public IEnumerator<Edge> GetEnumerator()
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
                    sb.Append(v._last_edge._from + " ");
                    first = false;
                }
                sb.Append("-> " + v._last_edge._to);
            }
            return sb.ToString();
        }
    }
}
