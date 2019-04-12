namespace PiggyGenerator
{
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using PiggyRuntime;

    public class Automaton
    {
        protected List<State> _all_states = new List<State>();
        protected List<Edge> _all_edges = new List<Edge>();
        protected SmartSet<State> _start_states = new SmartSet<State>();
        protected SmartSet<State> _start_states_subpattern = new SmartSet<State>();
        protected SmartSet<State> _final_states = new SmartSet<State>();
        protected SmartSet<State> _final_states_subpattern = new SmartSet<State>();

        public static bool IsLambdaTransition(Edge e)
        {
            if (e._c != null) return false;
            else if (e.IsAny) return false;
            else if (e.IsCode) return false;
            else if (e.IsText) return false;
            else if (e.IsNot) return false;
            else if (e.IsSubpattern) return false;
            else return true;
        }
        public IEnumerable<Edge> AllEdges()
        {
            return _all_edges;
        }
        public IEnumerable<Edge> AllEdges(State state)
        {
            return _all_edges.Where(e => e._from == state).ToList();
        }
        public IEnumerable<State> AllStates()
        {
            return _all_states;
        }
        public IEnumerable<State> StartStates { get { return _start_states; } }
        public IEnumerable<State> StartStatesSubpattern { get { return _start_states_subpattern; } }
        public IEnumerable<State> FinalStates { get { return _final_states; } }
        public IEnumerable<State> FinalStatesSubpattern { get { return _final_states_subpattern; } }
        public void AddState(State s)
        {
            if (_all_states.Contains(s)) return;
            _all_states.Add(s);
        }
        public void AddEdge(Edge e)
        {
            _all_edges.Add(e);
        }
        public void AddStartState(State ss)
        {
            if (_start_states.Contains(ss)) return;
            _start_states.Add(ss);
        }
        public void AddStartStateSubpattern(State ss)
        {
            if (_start_states_subpattern.Contains(ss)) return;
            _start_states_subpattern.Add(ss);
        }
        public void AddFinalState(State end)
        {
            if (_final_states.Contains(end)) return;
            _final_states.Add(end);
        }
        public void AddFinalStateSubpattern(State end)
        {
            if (_final_states_subpattern.Contains(end)) return;
            _final_states_subpattern.Add(end);
        }
        public bool IsFinalState(State state)
        {
            return _final_states.Contains(state);
        }
        public bool IsFinalStateSubpattern(State state)
        {
            return _final_states_subpattern.Contains(state);
        }
        public bool IsStartState(State state)
        {
            return _start_states.Contains(state);
        }
        public bool IsStartStateSubpattern(State state)
        {
            return _start_states_subpattern.Contains(state);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("digraph g {");
            foreach (var e in AllEdges())
            {
                sb.Append(e._from + " -> " + e._to
                    + " [label=\"");
                if (e.IsText)
                    sb.Append("[[ text ]]");
                else if (e.IsCode)
                    sb.Append("{{ code }}");
                else if (e.IsAny)
                    sb.Append(" any ");
                else if (e.IsSubpattern)
                    sb.Append(" subpattern-" + e._fragment_start.Id + " ");
                else if (e._c == null)
                    sb.Append(" empty ");
                else
                    sb.Append(e._c.provide_escapes());
                sb.AppendLine("\"];");
            }
            foreach (var ss in AllStates())
            {
                if (StartStates.Contains(ss)) sb.AppendLine(ss + " [shape=square];");
                else if (FinalStates.Contains(ss)) sb.AppendLine(ss + " [shape=Msquare];");
                else if (StartStatesSubpattern.Contains(ss)) sb.AppendLine(ss + " [shape=octagon];");
                else if (FinalStatesSubpattern.Contains(ss)) sb.AppendLine(ss + " [shape=doubleoctagon];");
                else sb.AppendLine(ss + " [shape=circle];");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
