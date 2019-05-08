namespace PiggyGenerator
{
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using PiggyRuntime;
    using Campy.Graphs;

    public class Automaton : GraphAdjList<State, Edge>
    {
        protected SmartSet<State> _start_states = new SmartSet<State>();
        protected SmartSet<State> _start_states_subpattern = new SmartSet<State>();
        protected SmartSet<State> _final_states = new SmartSet<State>();
        protected SmartSet<State> _final_states_subpattern = new SmartSet<State>();

        public static bool IsLambdaTransition(Edge e)
        {
            if (e.Input != null) return false;
            else if (e.IsAny) return false;
            else if (e.IsCode) return false;
            else if (e.IsText) return false;
            else if (e.IsNot) return false;
            else return true;
        }
        public IEnumerable<Edge> AllEdges()
        {
            return this.Edges;
        }
        public IEnumerable<Edge> AllEdges(State state)
        {
            return this.Edges.Where(e => e.From == state).ToList();
        }
        public IEnumerable<State> AllStates()
        {
            return this.Vertices;
        }
        public IEnumerable<State> StartStates { get { return _start_states; } }
        public IEnumerable<State> FinalStates { get { return _final_states; } }
        public void AddState(State s)
        {
            if (this.Vertices.Contains(s)) return;
            this.AddVertex(s);
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
            StringBuilder sb = new StringBuilder();
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
                else
                    sb.Append(e.Input.provide_escapes());
                sb.AppendLine("\"];");
            }
            foreach (var ss in AllStates())
            {
                if (StartStates.Contains(ss)) sb.AppendLine(ss + " [shape=square];");
                else if (FinalStates.Contains(ss)) sb.AppendLine(ss + " [shape=Msquare];");
                else sb.AppendLine(ss + " [shape=circle];");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
