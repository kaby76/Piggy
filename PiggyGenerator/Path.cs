using System.Collections;
using System.Collections.Generic;

namespace PiggyGenerator
{
    public class Path : IEnumerable<Edge>
    {
        readonly Path _next;
        readonly Edge _last_edge;

        public Path(Edge l)
        {
            _next = null;
            _last_edge = l;
        }

        public Path(Path n, Edge l)
        {
            _next = n;
            _last_edge = l;
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
    }
}
