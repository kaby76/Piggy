using System;
using Campy.Graphs;

namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    public class Edge : DirectedEdge<State>
    {
        public enum EdgeModifiers
        {
            DoNotUse = 0,
            Not = 1,
            Any = 2,
            Code = 4,
            Text = 8
        }

        public int _Id;
        private static int _id = 0;
        public State _from;
        public State _to;
        public string _input;
        private readonly Automaton _owner;
        public int _edge_modifiers;
        public static readonly List<IParseTree> EmptyAst = new List<IParseTree>();
        public static readonly string EmptyString = null;

        public Edge(Automaton owner, State @from, State to, IEnumerable<IParseTree> ast_list, int edge_modifiers = 0)
          : base(from, to)
        {
            _Id = ++_id;
            _owner = owner;
            _from = @from;
            _to = to;
            owner.AddEdge(this);
            AstList = ast_list;
            if (ast_list.Count() == 0) _input = EmptyString;
            else _input = ast_list.First().GetText();
            _edge_modifiers = edge_modifiers;
        }
        public bool IsAny
        {
            get
            {
                return 0 != (_edge_modifiers & (int)EdgeModifiers.Any);
            }
        }
        public bool IsNot
        {
            get
            {
                return 0 != (_edge_modifiers & (int)EdgeModifiers.Not);
            }
        }
        public bool IsText
        {
            get
            {
                return 0 != (_edge_modifiers & (int)EdgeModifiers.Text);
            }
        }
        public bool IsCode
        {
            get
            {
                return 0 != (_edge_modifiers & (int)EdgeModifiers.Code);
            }
        }
        public bool IsEmpty
        {
            get
            {
                return (!IsAny) && _input == Edge.EmptyString;
            }
        }
        public IEnumerable<IParseTree> AstList { get; protected set; }
        public override int GetHashCode()
        {
            return _from.Id + _to.Id * 16;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var o = obj as Edge;
            if (o == null) return false;
            if (this._from != o._from || this._to != o._to) return false;
            if (this._input != o._input) return false;
            if (this._edge_modifiers != o._edge_modifiers) return false;
            return true;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this._from + " -> " + this._to
                + " on '");
            if (this.IsAny) sb.Append("any");
            else if (this.IsCode) sb.Append("code");
            else if (this.IsText) sb.Append("text");
            else if (this._input == Edge.EmptyString) sb.Append("empty");
            else sb.Append(this._input);
            sb.Append("'");
            return sb.ToString();
        }
    }
}
