using System;

namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    public class Edge
    {
        public enum EdgeModifiers
        {
            DoNotUse = 0,
            Not = 1,
            Any = 2,
            Code = 4,
            Text = 8,
            Subpattern = 16
        }

        public State _from;
        public State _to;
        public string _c;
        public int _pattern_id;
        private Automaton _owner;
        public int _edge_modifiers;
        public State _fragment_start;
        public static readonly List<IParseTree> EmptyAst = new List<IParseTree>();
        public static readonly string EmptyString = null;

        private Edge() { }
        public Edge(Automaton owner, State @from, State to, State frag_start, IEnumerable<IParseTree> ast_list, int edge_modifiers = 0)
        {
            _owner = owner;
            _from = @from;
            _to = to;
            _fragment_start = frag_start;
            AstList = ast_list;
            if (ast_list.Count() == 0) _c = EmptyString;
            else _c = ast_list.First().GetText();
            _edge_modifiers = edge_modifiers;
            if (_fragment_start != null && !this.IsSubpattern) { throw new Exception(); }
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
                return (!IsAny) && (!IsSubpattern) && _c == Edge.EmptyString;
            }
        }
        public bool IsSubpattern
        {
            get
            {
                return 0 != (_edge_modifiers & (int)EdgeModifiers.Subpattern);
            }
        }

        public IEnumerable<IParseTree> AstList { get; protected set; }
        public void Commit()
        {
            _owner.AddEdge(this);
            _from._out_edges.Add(this);
        }
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
            if (this._c != o._c) return false;
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
            else if (this.IsSubpattern) sb.Append("subpat");
            else if (this._c == Edge.EmptyString) sb.Append("empty");
            else sb.Append(this._c);
            sb.Append("'");
            return sb.ToString();
        }
    }
}
