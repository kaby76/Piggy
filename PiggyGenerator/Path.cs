using System.Collections;
using System.Collections.Generic;

namespace PiggyGenerator
{
    public class Path : IEnumerable<Edge>
    {
        public Path(Edge l)
        {
            Next = null;
            LastEdge = l;
        }

        public Path(Path n, Edge l)
        {
            Next = n;
            LastEdge = l;
        }

        public Path Next
        {
            get; private set;
        }

        public Edge LastEdge
        {
            get; set;
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
