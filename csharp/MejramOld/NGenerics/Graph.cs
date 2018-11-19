/*  
  Copyright 2007-2009 The NGenerics Team
 (http://code.google.com/p/ngenerics/wiki/Team)

 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mejram.NGenerics
{
    /// <summary>
    /// An implementation of a Graph data structure.  The graph can be either
    /// directed or undirected.
    /// </summary>
    /// <typeparam name="T">The type of elements in the graph.</typeparam>
    [Serializable]
    public class Graph<T>
    {
        private readonly Dictionary<Vertex<T>, object> _graphVertices;
        private readonly Dictionary<Edge<T>, object> _graphEdges;
        private readonly bool _graphIsDirected;

        /// <param name="isDirected">if set to <c>true</c> [is directed].</param>
        public Graph(bool isDirected)
        {
            _graphIsDirected = isDirected;
            _graphVertices = new Dictionary<Vertex<T>, object>();
            _graphEdges = new Dictionary<Edge<T>, object>();
        }

        public bool IsEmpty
        {
            get { return _graphVertices.Count == 0; }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if ((array.Length - arrayIndex) < _graphVertices.Count)
            {
                throw new ArgumentException("Not enough space in the target array.", "array");
            }

            var counter = arrayIndex;

            foreach (var vertex in _graphVertices.Keys)
            {
                array.SetValue(vertex.Data, counter);
                counter++;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var vertexList = new List<Vertex<T>>(_graphVertices.Count);
            vertexList.AddRange(_graphVertices.Keys);

            for (var i = 0; i < vertexList.Count; i++)
            {
                var vertex = vertexList[i];
                yield return vertex.Data;
            }
        }

        public void Clear()
        {
            _graphVertices.Clear();
            _graphEdges.Clear();
        }

        /// <summary>
        /// Removes the specified vertex from the graph.
        /// </summary>
        /// <param name="vertex">The vertex to be removed.</param>
        /// <returns>A value indicating whether the vertex was found (and removed) in the graph.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="vertex"/> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
        public bool RemoveVertex(Vertex<T> vertex)
        {
            //no need to check vertex for null as graphVertices.Remove will do this
            if (!_graphVertices.Remove(vertex))
            {
                return false;
            }

            // Delete all the edges in which this vertex forms part of
            var list = vertex.IncidentEdges;

            while (list.Count > 0)
            {
                RemoveEdge(list[0]);
            }

            return true;
        }

        /// <summary>
        /// Removes the vertex with the specified value from the graph.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if a vertex with the value specified was found (and removed) in the graph; otherwise <c>false</c>.</returns>
        public bool RemoveVertex(T item)
        {
            foreach (var vertex in _graphVertices.Keys)
            {
                if (vertex.Data.Equals(item))
                {
                    RemoveVertex(vertex);
                    return true;
                }
            }

            return false;
        }

        public bool ContainsVertex(Vertex<T> vertex)
        {
            return _graphVertices.ContainsKey(vertex);
        }

        /// <summary>
        /// Determines whether the specified item is contained in the graph.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if the specified item contains vertex; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsVertex(T item)
        {
            foreach (var vertex in _graphVertices.Keys)
            {
                if (vertex.Data.Equals(item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is directed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is directed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirected
        {
            get { return _graphIsDirected; }
        }

        /// <summary>
        /// Removes the edge specified from the graph.
        /// </summary>
        /// <param name="edge">The edge to be removed.</param>
        /// <returns>A value indicating whether the edge specified was found (and removed) from the graph.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="edge"/> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
        public bool RemoveEdge(Edge<T> edge)
        {
            if (!_graphEdges.Remove(edge))
            {
                return false;
            }

            edge.FromVertex.RemoveEdge(edge);
            edge.ToVertex.RemoveEdge(edge);

            return true;
        }


        /// <summary>
        /// Removes the edge specified from the graph.
        /// </summary>
        /// <param name="from">The from vertex.</param>
        /// <param name="to">The to vertex.</param>
        /// <returns>A value indicating whether the edge between the two vertices supplied was found (and removed) from the graph.</returns>
        public bool RemoveEdge(Vertex<T> from, Vertex<T> to)
        {
            if (_graphIsDirected)
            {
                foreach (var edge in _graphEdges.Keys)
                {
                    if ((edge.FromVertex == from) && (edge.ToVertex == to))
                    {
                        RemoveEdge(edge);
                        return true;
                    }
                }
            }
            else
            {
                foreach (var edge in _graphEdges.Keys)
                {
                    if (((edge.FromVertex == from) && (edge.ToVertex == to)) ||
                        ((edge.FromVertex == to) && (edge.ToVertex == from)))
                    {
                        RemoveEdge(edge);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Adds the specified edge to the graph.
        /// </summary>
        public void AddEdge(Edge<T> edge)
        {
            if (edge.IsDirected != _graphIsDirected)
            {
                throw new ArgumentException(
                    "The type of edge must be the same as the type of graph (Undirected / Directed)", "edge");
            }

            if ((!_graphVertices.ContainsKey(edge.FromVertex)) || (!_graphVertices.ContainsKey(edge.ToVertex)))
            {
                throw new ArgumentException("The vertex specified could not be found in the graph.", "edge");
            }

            if (edge.FromVertex.HasEmanatingEdgeTo(edge.ToVertex))
            {
                throw new ArgumentException("The edge between the vertices specified already exists.", "edge");
            }
            _graphEdges.Add(edge, null);
            AddEdgeToVertices(edge);
        }

        /// <summary>
        /// Adds the vertex specified to the graph.
        /// </summary>
        /// <param name="vertex">The vertex to add.</param>
        public void AddVertex(Vertex<T> vertex)
        {
            if (_graphVertices.ContainsKey(vertex))
            {
                throw new ArgumentException("The vertex already exists in the graph.", "vertex");
            }
            _graphVertices.Add(vertex, null);
        }

        /// <summary>
        /// Adds a vertex to the graph with the specified data item.
        /// </summary>
        public Vertex<T> AddVertex(T item)
        {
            var vertex = new Vertex<T>(item);
            _graphVertices.Add(vertex, null);
            return vertex;
        }

        /// <summary>
        /// Adds the edge to the graph.
        /// </summary>
        /// <param name="from">The from vertex.</param>
        /// <param name="to">The to vertex.</param>
        /// <returns>The newly created <see cref="Edge{T}"/>.</returns>
        public Edge<T> AddEdge(Vertex<T> from, Vertex<T> to)
        {
            var edge = new Edge<T>(from, to, _graphIsDirected);
            AddEdge(edge);
            return edge;
        }

        /// <summary>
        /// Adds the edge to the graph.
        /// </summary>
        /// <param name="from">The from vertex.</param>
        /// <param name="to">The to vertex.</param>
        /// <param name="weight">The weight of this edge.</param>
        /// <returns>The newly created <see cref="Edge{T}"/>.</returns>
        public Edge<T> AddEdge(Vertex<T> from, Vertex<T> to, double weight)
        {
            var edge = new Edge<T>(from, to, weight, _graphIsDirected);
            AddEdge(edge);
            return edge;
        }

        /// <summary>
        /// Gets the vertices contained in this graph.
        /// </summary>
        /// <value>The vertices contained in this graph.</value>
        public ICollection<Vertex<T>> Vertices
        {
            get { return _graphVertices.Keys; }
        }

        /// <summary>
        /// Gets the edges contained in this graph.
        /// </summary>
        /// <value>The edges contained in this graph.</value>
       public ICollection<Edge<T>> Edges
        {
            get { return _graphEdges.Keys; }
        }


        /// <summary>
        /// Determines whether the vertex with the specified from value has an edge to a vertex with the specified to value.
        /// </summary>
        /// <param name="fromValue">The from vertex value.</param>
        /// <param name="toValue">The to vertex value.</param>
        /// <returns>
        /// 	<c>true</c> if the vertex with the specified from value has an edge to a vertex with the specified to value; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsEdge(T fromValue, T toValue)
        {
            if (_graphIsDirected)
            {
                foreach (var edge in _graphEdges.Keys)
                {
                    if ((edge.FromVertex.Data.Equals(fromValue) &&
                         (edge.ToVertex.Data.Equals(toValue))))
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (var edge in _graphEdges.Keys)
                {
                    if (((edge.FromVertex.Data.Equals(fromValue) &&
                          (edge.ToVertex.Data.Equals(toValue)))) ||
                        ((edge.FromVertex.Data.Equals(toValue) &&
                          (edge.ToVertex.Data.Equals(fromValue)))))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified vertex has a edge to the to vertex.
        /// </summary>
        /// <param name="from">The from vertex.</param>
        /// <param name="to">The to vertex.</param>
        /// <returns>
        /// 	<c>true</c> if the specified from vertex has an edge to the to vertex; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsEdge(Vertex<T> from, Vertex<T> to)
        {
            return _graphIsDirected ? from.HasEmanatingEdgeTo(to) : from.HasIncidentEdgeWith(to);
        }

        /// <summary>
        /// Determines whether the specified edge is contained in this graph.
        /// </summary>
        /// <param name="edge">The edge to look for.</param>
        /// <returns>
        /// 	<c>true</c> if the specified edge is contained in the graph; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsEdge(Edge<T> edge)
        {
            return _graphEdges.ContainsKey(edge);
        }

        /// <summary>
        /// Gets the edge specified by the two vertices.
        /// </summary>
        /// <param name="from">The from vertex.</param>
        /// <param name="to">The two vertex.</param>
        /// <returns>The <see cref="Edge{T}"/> between the two specified vertices if found; otherwise a null reference.</returns>
        public Edge<T> GetEdge(Vertex<T> from, Vertex<T> to)
        {
            return from.GetEmanatingEdgeTo(to);
        }

        /// <summary>
        /// Gets the edge specified by the two vertices.
        /// </summary>
        /// <param name="fromVertexValue">The from vertex value.</param>
        /// <param name="toVertexValue">The to vertex value.</param>
        /// <returns>The <see cref="Edge{T}"/> formed by vertices with the specified values if found, otherwise a null reference.</returns>
        public Edge<T> GetEdge(T fromVertexValue, T toVertexValue)
        {
            foreach (var vertex in _graphVertices.Keys)
            {
                if (vertex.Data.Equals(fromVertexValue))
                {
                    for (var i = 0; i < vertex.EmanatingEdges.Count; i++)
                    {
                        var edge = vertex.EmanatingEdges[i];
                        var partner = edge.GetPartnerVertex(vertex);

                        if (partner.Data.Equals(toVertexValue))
                        {
                            return edge;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the vertex with the specified value.
        /// </summary>
        /// <param name="vertexValue">The vertex value to look for.</param>
        /// <returns>The <see cref="Vertex{T}"/> with the specified value.</returns>

        public Vertex<T> GetVertex(T vertexValue)
        {
            foreach (var vertex in _graphVertices.Keys)
            {
                if (vertex.Data.Equals(vertexValue))
                {
                    return vertex;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds the edge to the vertices in the edge.
        /// </summary>
        /// <param name="edge">The edge to add.</param>
        private static void AddEdgeToVertices(Edge<T> edge)
        {
            Debug.Assert(edge != null);
            Debug.Assert(edge.FromVertex != null);
            Debug.Assert(edge.ToVertex != null);

            edge.FromVertex.AddEdge(edge);
            edge.ToVertex.AddEdge(edge);
        }

       
        /// <inheritdoc />
        /// <value>
        /// 	Always <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}