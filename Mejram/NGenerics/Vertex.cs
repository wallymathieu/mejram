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
using System.Collections.ObjectModel;

namespace Mejram.NGenerics
{
    /// <summary>
    /// A class representing a vertex in a graph.
    /// </summary>
    /// <typeparam name="T">The type contained in the vertex.</typeparam>
    [Serializable]
    public class Vertex<T> : IEquatable<Vertex<T>>
    {
        private readonly List<Edge<T>> _incidentEdges;
        private readonly List<Edge<T>> _emanatingEdges;
        /// <remarks>The weight is 0 by default.</remarks>
        /// <param name="data">The data contained in the vertex.</param>
        public Vertex(T data)
        {
            Data = data;
            _incidentEdges = new List<Edge<T>>();
            _emanatingEdges = new List<Edge<T>>();
            Weight = 0;
        }


        /// <param name="data">The data contained in the vertex</param>
        /// <param name="weight">The weight of the vertex.</param>
        public Vertex(T data, double weight)
        {
            Data = data;
            _incidentEdges = new List<Edge<T>>();
            _emanatingEdges = new List<Edge<T>>();
            Weight = weight;
        }

        /// <summary>
        /// Gets or sets the weight.
        /// </summary>
        /// <value>The weight.</value>
        public double Weight { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data contained in the vertex.</value>
        public T Data { get; set; }

        /// <summary>
        /// Gets the degree of this vertex (the number of emanating edges).
        /// </summary>
        /// <value>The degree.</value>
        public int Degree
        {
            get { return _emanatingEdges.Count; }
        }

        /// <summary>
        /// Gets the edges incident on this vertex.
        /// </summary>
        /// <value>The edges incident on this vertex.</value>
        public IList<Edge<T>> IncidentEdges
        {
            get { return new ReadOnlyCollection<Edge<T>>(_incidentEdges); }
        }

        /// <summary>
        /// Gets the emanating edges on this vertex.
        /// </summary>
        /// <value>The emanating edges on this vertex.</value>
        public IList<Edge<T>> EmanatingEdges
        {
            get { return new ReadOnlyCollection<Edge<T>>(_emanatingEdges); }
        }

        /// <summary>
        /// Gets count of the incoming edges on this vertex.
        /// </summary>
        /// <value>The number of incoming edges resident on the vertex.</value>
        public int IncomingEdgeCount
        {
            get { return _incidentEdges.Count - _emanatingEdges.Count; }
        }

        /// <summary>
        /// Determines whether this vertex has an emanating edge to the specified vertex.
        /// </summary>
        /// <param name="toVertex">The vertex to test connectivity to.</param>
        /// <returns>
        /// 	<c>true</c> if this vertex has an emanating edge to the specified vertex; otherwise, <c>false</c>.
        /// </returns>
        public bool HasEmanatingEdgeTo(Vertex<T> toVertex)
        {
            for (var i = 0; i < _emanatingEdges.Count; i++)
            {
                if (_emanatingEdges[i].IsDirected)
                {
                    if (_emanatingEdges[i].ToVertex == toVertex)
                    {
                        return true;
                    }
                }
                else
                {
                    if ((_emanatingEdges[i].ToVertex == toVertex) || ((_emanatingEdges[i].FromVertex == toVertex)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether [has incident edge with] [the specified from vertex].
        /// </summary>
        /// <param name="fromVertex">From vertex.</param>
        /// <returns>
        /// 	<c>true</c> if [has incident edge with] [the specified from vertex]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasIncidentEdgeWith(Vertex<T> fromVertex)
        {
            for (var i = 0; i < _incidentEdges.Count; i++)
            {
                if ((_incidentEdges[i].FromVertex == fromVertex) || (_incidentEdges[i].ToVertex == fromVertex))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the emanating edge to the specified vertex.
        /// </summary>
        /// <param name="toVertex">To to vertex.</param>
        /// <returns>The emanating edge to the vertex specified if found, otherwise null.</returns>
        public Edge<T> GetEmanatingEdgeTo(Vertex<T> toVertex)
        {
            for (var i = 0; i < _emanatingEdges.Count; i++)
            {
                if (_emanatingEdges[i].IsDirected)
                {
                    if (_emanatingEdges[i].ToVertex == toVertex)
                    {
                        return _emanatingEdges[i];
                    }
                }
                else
                {
                    if ((_emanatingEdges[i].FromVertex == toVertex) || (_emanatingEdges[i].ToVertex == toVertex))
                    {
                        return _emanatingEdges[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the incident edge to the specified vertex.
        /// </summary>
        /// <param name="toVertex">The to vertex.</param>
        /// <returns>The incident edge to the vertex specified if found, otherwise null.</returns>
        public Edge<T> GetIncidentEdgeWith(Vertex<T> toVertex)
        {
            for (var i = 0; i < _incidentEdges.Count; i++)
            {
                if ((_incidentEdges[i].ToVertex == toVertex) || (_incidentEdges[i].FromVertex == toVertex))
                {
                    return _incidentEdges[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Removes the edge specified from the vertex.
        /// </summary>
        /// <param name="edge">The edge to be removed.</param>
        internal void RemoveEdge(Edge<T> edge)
        {
            Debug.Assert(edge != null);
            RemoveEdgeFromVertex(edge);
        }

        /// <summary>
        /// Adds the edge to the vertex.
        /// </summary>
        /// <param name="edge">The edge to add.</param>
        internal void AddEdge(Edge<T> edge)
        {
            Debug.Assert(edge != null);
            if (edge.IsDirected)
            {
                if (edge.FromVertex == this)
                {
                    _emanatingEdges.Add(edge);
                }
            }
            else
            {
                _emanatingEdges.Add(edge);
            }

            _incidentEdges.Add(edge);
        }
        private void RemoveEdgeFromVertex(Edge<T> edge)
        {
            _incidentEdges.Remove(edge);

            if (edge.IsDirected)
            {
                if (edge.FromVertex == this)
                {
                    _emanatingEdges.Remove(edge);
                }
            }
            else
            {
                _emanatingEdges.Remove(edge);
            }
        }

        public bool Equals(Vertex<T> other)
        {
            if (null == other) return false;
            return Data.Equals(other.Data);
        }

        public override bool Equals(object obj)
        {
            return obj is Vertex<T> && this.Equals((Vertex<T>) obj);
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }
    }
}