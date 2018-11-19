/*  
  Copyright 2007-2009 The NGenerics Team
 (http://code.google.com/p/ngenerics/wiki/Team)

 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/


using System;

namespace Mejram.NGenerics
{
    /// <summary>
    /// A class representing an edge in a graph.
    /// </summary>
    /// <typeparam name="T">The type of object the edge contains.</typeparam>
    [Serializable]
    public class Edge<T>
    {
        public Vertex<T> From { get; private set; }
        public Vertex<T> To { get; private set; }
        public bool IsDirected { get; private set; }


        /// <param name="fromVertex">From <see cref="Vertex{T}"/>.</param>
        /// <param name="toVertex">To <see cref="Vertex{T}"/>.</param>
        /// <param name="isDirected">if set to <c>true</c> [is directed].</param>
        public Edge(Vertex<T> fromVertex, Vertex<T> toVertex, bool isDirected)
            : this(fromVertex, toVertex, 0, isDirected)
        {
        }


        /// <param name="fromVertex">From <see cref="Vertex{T}"/>.</param>
        /// <param name="toVertex">To <see cref="Vertex{T}"/>.</param>
        /// <param name="weight">The weight associated with the edge.</param>
        /// <param name="isDirected">if set to <c>true</c> [is directed].</param>
        public Edge(Vertex<T> fromVertex, Vertex<T> toVertex, double weight, bool isDirected)
        {
            if (toVertex == null)
                throw new ArgumentNullException("toVertex");
            if (fromVertex == null)
                throw new ArgumentNullException("fromVertex");

            From = fromVertex;
            To = toVertex;
            Weight = weight;
            IsDirected = isDirected;
        }


        /// <summary>
        /// Gets the from <see cref="Vertex{T}"/>.
        /// </summary>
        /// <value>The from <see cref="Vertex{T}"/>.</value>
        public Vertex<T> FromVertex
        {
            get { return From; }
        }

        /// <summary>
        /// Gets the to <see cref="Vertex{T}"/>.
        /// </summary>
        /// <value>The to <see cref="Vertex{T}"/>.</value>
        public Vertex<T> ToVertex
        {
            get { return To; }
        }

        /// <summary>
        /// Gets the weight associated with this <see cref="Edge{T}"/>.
        /// </summary>
        /// <value>The weight associated with this <see cref="Edge{T}"/>.</value>
        public double Weight { get; set; }


        /// <summary>
        /// Gets the partner vertex in this <see cref="Edge{T}"/> relationship.
        /// </summary>
        /// <param name="vertex">The <see cref="Vertex{T}"/>.</param>
        /// <returns>The partner of the <see cref="Vertex{T}"/> specified in this <see cref="Edge{T}"/> relationship.</returns>
        /// <exception cref="ArgumentException"><paramref name="vertex"/> does not equal <see cref="FromVertex"/> or <see cref="ToVertex"/>.</exception>
        public Vertex<T> GetPartnerVertex(Vertex<T> vertex)
        {
            if (From == vertex)
                return To;

            if (To == vertex)
                return From;

            throw new ArgumentException("The vertex specified does not form part of this edge.", "vertex");
        }


        /// <summary>
        /// Gets or sets the tag object contained in the edge.
        /// </summary>
        /// <value>The tag object.</value>
        public object Tag { get; set; }
    }
}