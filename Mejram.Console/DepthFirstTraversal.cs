using System;
using System.IO;
using System.Text;
using NGenerics.DataStructures.General;

namespace DataConsole
{
  internal class DepthFirstTraversal
  {
    public TextWriter outv = null;
    System.Collections.Generic.HashSet<Vertex<String>> visited = new System.Collections.Generic.HashSet<Vertex<string>>();
    public void Dfs(Vertex<String> v, Action<Vertex<String>> preorder_process, Action<Vertex<String>> postorder_process, int depth)
    {
      visited.Add(v);
      Print(depth, v.Data);
      preorder_process(v);
      foreach (var neighbor in v.EmanatingEdges)
      {
        var partnerVertex = neighbor.GetPartnerVertex(v);
        if (!visited.Contains(partnerVertex))
        {
          Dfs(partnerVertex, preorder_process, postorder_process, depth + 1);
        }
      }
      postorder_process(v);
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