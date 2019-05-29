using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using Campy.Graphs;

namespace Engine
{
    public class Edge : DirectedEdge<State>
    {
        public enum EdgeModifiersEnum
        {
            DoNotUse = 0,
            Not = 1,
            Any = 2,
            Code = 4,
            Text = 8
        }

        private static int _next_id;
        public static readonly List<IParseTree> EmptyAst = new List<IParseTree>();
        public static readonly string EmptyString = null;
        private readonly Automaton _owner;

        public Edge(Automaton owner, State from, State to, IEnumerable<IParseTree> ast_list, int edge_modifiers = 0)
            : base(from, to)
        {
            Id = ++_next_id;
            _owner = owner;
            AstList = ast_list;
            if (ast_list.Count() == 0) Input = EmptyString;
            else Input = ast_list.First().GetText();
            EdgeModifiers = edge_modifiers;
            owner.AddEdge(this);
        }

        public IEnumerable<IParseTree> AstList { get; protected set; }

        public int EdgeModifiers { get; }

        public int Id { get; }

        public string Input { get; }

        public bool IsAny => 0 != (EdgeModifiers & (int) EdgeModifiersEnum.Any);

        public bool IsCode => 0 != (EdgeModifiers & (int) EdgeModifiersEnum.Code);

        public bool IsEmpty => !IsAny && !IsCode && !IsText && Input == EmptyString;

        public bool IsNot => 0 != (EdgeModifiers & (int) EdgeModifiersEnum.Not);

        public bool IsText => 0 != (EdgeModifiers & (int) EdgeModifiersEnum.Text);

        public override int GetHashCode()
        {
            return From.Id + To.Id * 16;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var o = obj as Edge;
            if (o == null) return false;
            if (From != o.From || To != o.To) return false;
            if (Input != o.Input) return false;
            if (EdgeModifiers != o.EdgeModifiers) return false;
            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(From + " -> " + To
                      + " on '");
            if (IsAny) sb.Append("any");
            else if (IsCode) sb.Append("code");
            else if (IsText) sb.Append("text");
            else if (Input == EmptyString) sb.Append("empty");
            else
            {
                if (IsNot) sb.Append("!");
                sb.Append(Input);
            }
            sb.Append("'");
            return sb.ToString();
        }
    }
}