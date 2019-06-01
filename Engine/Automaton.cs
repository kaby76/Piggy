using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graphs;
using Runtime;

namespace Engine
{
    public class Automaton : GraphAdjList<State, Edge>
    {
        protected SmartSet<State> _final_states = new SmartSet<State>();
        protected SmartSet<State> _final_states_subpattern = new SmartSet<State>();
        protected SmartSet<State> _start_states = new SmartSet<State>();
        protected SmartSet<State> _start_states_subpattern = new SmartSet<State>();
        public IEnumerable<State> FinalStates => _final_states;
        public IEnumerable<State> StartStates => _start_states;

        public static bool IsEpsilonTransition(Edge e)
        {
            if (e.Input != null) return false;
            if (e.IsAny) return false;
            if (e.IsNot) return false;
            return true;
        }

        public IEnumerable<Edge> AllEdges()
        {
            return Edges;
        }

        public IEnumerable<Edge> AllEdges(State state)
        {
            return Edges.Where(e => e.From == state).ToList();
        }

        public IEnumerable<State> AllStates()
        {
            return Vertices;
        }

        public void AddState(State s)
        {
            if (Vertices.Contains(s)) return;
            AddVertex(s);
        }

        public void AddStartState(State ss)
        {
            if (_start_states.Contains(ss)) return;
            _start_states.Add(ss);
        }

        public void AddFinalState(State end)
        {
            if (_final_states.Contains(end)) return;
            _final_states.Add(end);
        }

        public bool IsFinalState(State state)
        {
            return _final_states.Contains(state);
        }

        public bool IsStartState(State state)
        {
            return _start_states.Contains(state);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph g {");
            foreach (var e in AllEdges())
            {
                sb.Append(e.From + " -> " + e.To
                          + " [label=\"");
                if (e.IsText)
                    sb.Append("[[ text ]]");
                else if (e.IsCode)
                    sb.Append("{{ code }}");
                else if (e.IsAny)
                    sb.Append(" any ");
                else if (e.Input == null)
                    sb.Append(" empty ");
                else if (e.IsNot)
                    sb.Append("!"+e.Input.provide_escapes());
                else
                    sb.Append(e.Input.provide_escapes());
                sb.AppendLine("\"];");
            }

            foreach (var ss in AllStates())
                if (StartStates.Contains(ss))
                    sb.AppendLine(ss + " [shape=square];");
                else if (FinalStates.Contains(ss)) sb.AppendLine(ss + " [shape=Msquare];");
                else sb.AppendLine(ss + " [shape=circle];");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}