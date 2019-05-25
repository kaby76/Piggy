using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Tree;

namespace PiggyGenerator
{
    public class Path : IEnumerable<Path>
    {
        public Path() { }

        public Path(Edge e, IParseTree input)
        {
            Next = null;
            LastEdge = e;
            Input = input;
            InputText = input.GetText();
        }

        public Path(Path n, Edge e, IParseTree input)
        {
            Next = n;
            if (n == null) throw new Exception();
            LastEdge = e;
            Input = input;
            InputText = input != null ? input.GetText() : "";
        }

        public Path(Edge e, IParseTree input, Path r)
        {
            if (r == null) throw new Exception();
            var p = r;
            while (p.Next != null) p = p.Next;
            p.Next = this;
            LastEdge = e;
            Input = input;
            InputText = input != null ? input.GetText() : "";
            var ss = this.ToString();
            System.Console.Error.WriteLine(ss);
            System.Console.Error.WriteLine(r);
        }

        public IParseTree Input { get; }

        public string InputText { get; }

        public Edge LastEdge { get; }

        public Path Next { get; private set; }

        public IEnumerator<Path> GetEnumerator()
        {
            return Doit();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Doit();
        }

        private IEnumerator<Path> Doit()
        {
            // Follow Next link to get path in reverse.
            // Then, iterate over each Path object to get the total
            // path in a forward direction.
            var stack = new Stack<Path>();
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

        public override string ToString()
        {
            var sb = new StringBuilder();
            var stack = new Stack<Path>();
            var p = this;
            while (p != null)
            {
                stack.Push(p);
                p = p.Next;
            }

            var first = true;
            while (stack.Count > 0)
            {
                var v = stack.Pop();
                if (first)
                {
                    sb.Append(v.LastEdge.From);
                    first = false;
                }

                sb.Append(" -> " + v.LastEdge.To);
            }

            return sb.ToString();
        }
    }
}