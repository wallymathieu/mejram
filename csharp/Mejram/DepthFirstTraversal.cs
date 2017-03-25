using System;
using System.IO;
using System.Text;
using Mejram.NGenerics;

namespace Mejram
{
    public class DepthFirstTraversal
    {
        public TextWriter outv = null;

        private readonly System.Collections.Generic.HashSet<Vertex<string>> _visited =
            new System.Collections.Generic.HashSet<Vertex<string>>();

        public void Dfs(Vertex<String> v, Action<Vertex<String>> preorderProcess,
                        Action<Vertex<String>> postorderProcess, int depth)
        {
            _visited.Add(v);
            Print(depth, v.Data);
            preorderProcess(v);
            foreach (var neighbor in v.EmanatingEdges)
            {
                var partnerVertex = neighbor.GetPartnerVertex(v);
                if (!_visited.Contains(partnerVertex))
                {
                    Dfs(partnerVertex, preorderProcess, postorderProcess, depth + 1);
                }
            }
            postorderProcess(v);
        }

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
    }
}