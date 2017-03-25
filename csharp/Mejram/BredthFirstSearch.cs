/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mejram.NGenerics;

namespace Mejram
{
    public class BredthFirstSearch
    {
		public TextWriter outv = null;

        public void Print(int depth, string text)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < depth; i++)
            {
                sb.Append("  ");
            }
            sb.Append(text);
            outv.WriteLine(sb.ToString());
        }

        private readonly List<Dictionary<Vertex<string>, Edge<string>>> _l =
            new List<Dictionary<Vertex<string>, Edge<string>>>();

        private readonly HashSet<Edge<String>> _exploredEdges = new HashSet<Edge<string>>();
        private readonly HashSet<Edge<String>> _crossEdges = new HashSet<Edge<string>>();
        private readonly HashSet<Vertex<String>> _exploredVertex = new HashSet<Vertex<string>>();
        //readonly List<List<Vertex<String>>> _paths = new List<List<Vertex<string>>>();
        private Vertex<string> end;

        public BredthFirstSearch(Vertex<string> vertex, Vertex<string> end)
        {
            _l.Add(new Dictionary<Vertex<string>, Edge<string>> {{vertex, null}});
            this.end = end;
        }

        public int maxDepth = 10000;
        private List<KeyValuePair<Vertex<string>, int>> _found = new List<KeyValuePair<Vertex<string>, int>>();

        public BredthFirstSearch Bfs()
        {
            int i = 0;

            while (_l[i].Count > 0)
            {
                _l.Add(new Dictionary<Vertex<string>, Edge<string>>());
                foreach (var v in _l[i])
                {
                    Print(i, v.Key.Data);
                    foreach (var e in v.Key.IncidentEdges)
                    {
                        //var vertex = e.GetPartnerVertex(v);
                        if (!_exploredEdges.Contains(e))
                        {
                            var w = e.GetPartnerVertex(v.Key);
                            if (!_exploredVertex.Contains(w))
                            {
                                _exploredEdges.Add(e);
                                _exploredVertex.Add(w);
                                _l[i + 1].Add(w, e);
                                if (w.HasIncidentEdgeWith(end))
                                {
                                    _found.Add(new KeyValuePair<Vertex<string>, int>(w, i + 1));
                                }
                            }
                            else
                                _crossEdges.Add(e);
                        }
                    }
                }
                i++;
                if (i >= maxDepth)
                {
                    Console.WriteLine("to deep");
                    break;
                }
            }
            return this;
        }

        public void ConstructPaths()
        {
            foreach (var foundIndex in _found)
            {
                Console.Write("" + foundIndex.Key.Data);
                var node = foundIndex.Key;
                var index = foundIndex.Value;
                while (index > 0)
                {
                    var parent = _l[index][node];

                    node = parent.GetPartnerVertex(node);
                    Console.Write(" {0} -> {1}", parent.Tag, node.Data);
                    index--;
                }
                Console.WriteLine();
            }
        }

        public bool Found()
        {
            return _found.Count > 0;
        }
    }
}