﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graphs
{
    /******************************************************************************
     *  Compilation:  javac Digraph.java
     *  Execution:    java Digraph filename.txt
     *  Dependencies: Bag.java In.java StdOut.java
     *  Data files:   https://algs4.cs.princeton.edu/42digraph/tinyDG.txt
     *                https://algs4.cs.princeton.edu/42digraph/mediumDG.txt
     *                https://algs4.cs.princeton.edu/42digraph/largeDG.txt  
     *
     *  A graph, implemented using an array of lists.
     *  Parallel edges and self-loops are permitted.
     *
     *  % java Digraph tinyDG.txt
     *  13 vertices, 22 edges
     *  0: 5 1 
     *  1: 
     *  2: 0 3 
     *  3: 5 2 
     *  4: 3 2 
     *  5: 4 
     *  6: 9 4 8 0 
     *  7: 6 9
     *  8: 6 
     *  9: 11 10 
     *  10: 12 
     *  11: 4 12 
     *  12: 9 
     *  
     ******************************************************************************/

    /**
     *  The {@code Digraph} class represents a directed graph of vertices
     *  named 0 through <em>V</em> - 1.
     *  It supports the following two primary operations: add an edge to the digraph,
     *  iterate over all of the vertices adjacent from a given vertex.
     *  Parallel edges and self-loops are permitted.
     *  <p>
     *  This implementation uses an adjacency-lists representation, which 
     *  is a vertex-indexed array of {@link Bag} objects.
     *  All operations take constant time (in the worst case) except
     *  iterating over the vertices adjacent from a given vertex, which takes
     *  time proportional to the number of such vertices.
     *  <p>
     *  For additional documentation,
     *  see <a href="https://algs4.cs.princeton.edu/42digraph">Section 4.2</a> of
     *  <i>Algorithms, 4th Edition</i> by Robert Sedgewick and Kevin Wayne.
     *
     *  @author Robert Sedgewick
     *  @author Kevin Wayne
     */

    public class Digraph<T> : GraphAdjList<T, DirectedEdge<T>> where T : new()
    {
        private const String NEWLINE = "\n";

        public Digraph()
        {
        }

        /**  
         * Initializes a digraph from the specified input stream.
         * The format is the number of vertices <em>V</em>,
         * followed by the number of edges <em>E</em>,
         * followed by <em>E</em> pairs of vertices, with each entry separated by whitespace.
         *
         * @param  in the input stream
         * @throws Exception if the endpoints of any edge are not in prescribed range
         * @throws Exception if the number of vertices or edges is negative
         * @throws Exception if the input stream is in the wrong format
         */
        public delegate T Parse(string value);
        public Digraph(string content, Parse parser)
        {
            try
            {
                string[] strings = content.Split(new char[] {' ', '\t', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                var vertex_count = Int32.Parse(strings[0]);
                var edge_count = Int32.Parse(strings[1]);

                if (vertex_count < 0)
                    throw new Exception("number of vertices in a Digraph must be nonnegative");
                if (edge_count < 0)
                    throw new Exception("number of edges in a Digraph must be nonnegative");
                for (int v = 0; v < edge_count; v++)
                {
                    var f = parser(strings[2 + v]);
                    var t = parser(strings[2 + v + 1]);
                    this.AddVertex(f);
                    this.AddVertex(t);
                    addEdge(new DirectedEdge<T>(f, t));
                }
            }
            catch (Exception e)
            {
                throw new Exception("invalid input format in Digraph constructor", e);
            }
        }

        /**
         * Initializes a new digraph that is a deep copy of the specified digraph.
         * 
         * KED: Note==this isn't a "deep copy" because a node in Sedgewick/Wayne is just an integer, not a class.
         * Integers are value types. Just make a copy of the graph using the same naming scheme.
         *
         * @param  G the digraph to copy
         */
        public Digraph(Digraph<T> G)
        {
            foreach (var n in G.Vertices)
            {
                this.AddVertex(n);
            }
            foreach (var e in G.Edges)
            {
                this.AddEdge(e);
            }
        }

        /**
         * Returns the number of vertices in this digraph.
         *
         * @return the number of vertices in this digraph
         */
        public int V
        {
            get { return this.Vertices.Count(); }
        }

        /**
         * Returns the number of edges in this digraph.
         *
         * @return the number of edges in this digraph
         */
        public int E
        {
            get { return this.Edges.Count(); }
        }


        // throw an IllegalArgumentException unless {@code 0 <= v < V}
        private void validateVertex(T v)
        {
            //if (v < 0 || v >= V)
            //    throw new Exception("vertex " + v + " is not between 0 and " + (V - 1));
        }

        /**
         * Adds the directed edge v→w to this digraph.
         *
         * @param  v the tail vertex
         * @param  w the head vertex
         * @throws IllegalArgumentException unless both {@code 0 <= v < V} and {@code 0 <= w < V}
         */
        public void addEdge(DirectedEdge<T> e)
        {
            validateVertex(e.From);
            validateVertex(e.To);
            base.AddEdge(e);
        }

        /**
         * Returns the vertices adjacent from vertex {@code v} in this digraph.
         *
         * @param  v the vertex
         * @return the vertices adjacent from vertex {@code v} in this digraph, as an iterable
         * @throws Exception unless {@code 0 <= v < V}
         */
        public IEnumerable<T> adj(T v)
        {
            validateVertex(v);
            return Successors(v);
        }

        /**
         * Returns the number of directed edges incident from vertex {@code v}.
         * This is known as the <em>outdegree</em> of vertex {@code v}.
         *
         * @param  v the vertex
         * @return the outdegree of vertex {@code v}               
         * @throws Exception unless {@code 0 <= v < V}
         */
        public int outdegree(T v)
        {
            validateVertex(v);
            return adj(v).Count();
        }

        /**
         * Returns the number of directed edges incident to vertex {@code v}.
         * This is known as the <em>indegree</em> of vertex {@code v}.
         *
         * @param  v the vertex
         * @return the indegree of vertex {@code v}               
         * @throws IllegalArgumentException unless {@code 0 <= v < V}
         */
        public int indegree(T v)
        {
            validateVertex(v);
            return this.Predecessors(v).Count();
        }

        /**
         * Returns the reverse of the digraph.
         *
         * @return the reverse of the digraph
         */
        public Digraph<T> reverse()
        {
            Digraph<T> reverse = new Digraph<T>();
            foreach (var v in this.Vertices)
            {
                var nv = new T();
            }
            foreach (var v in this.Vertices)
            {
                foreach (var s in this.Successors(v))
                {
                    reverse.AddEdge(new DirectedEdge<T>(s, v));
                }
            }
            return reverse;
        }

        /**
         * Returns a string representation of the graph.
         *
         * @return the number of vertices <em>V</em>, followed by the number of edges <em>E</em>,  
         *         followed by the <em>V</em> adjacency lists
         */
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(V.ToString() + " vertices, " + E + " edges " + NEWLINE);
            foreach (var v in this.Vertices)
            {
                s.Append(String.Format("{0}: ", v));
                foreach (T w in adj(v))
                {
                    s.Append(String.Format("{0} ", w));
                }
                s.Append(NEWLINE);
            }
            return s.ToString();
        }

        /**
         * Unit tests the {@code Digraph} data type.
         *
         * @param args the command-line arguments
         */
        public static void test()
        {
            string input = $@"
13
22
 4  2
 2  3
 3  2
 6  0
 0  1
 2  0
11 12
12  9
 9 10
 9 11
 7  9
10 12
11  4
 4  3
 3  5
 6  8
 8  6
 5  4
 0  5
 6  4
 6  9
 7  6
";

            var graph = new Digraph<int>(input, (string s) => System.Int32.Parse(s));
            System.Console.WriteLine(graph);
        }
    }
}
