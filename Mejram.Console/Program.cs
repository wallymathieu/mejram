using System;
using System.Collections.Generic;
using System.Linq;
using Mejram.Model;
using Mejram.NGenerics;

namespace Mejram.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (null != args.FirstOrDefault(a => a == "-serialize"))
            {
                Serialization.Serialize();
            }
            if (null != args.FirstOrDefault(a => a == "-graphfind"))
            {
                IEnumerable<ForeignKeyConstraint> foreignKeys;
                IEnumerable<Table> tables;
                var v = Serialization.Deserialize();
                var constraints = v.X2.Union(v.X3)
                    .Where(fk => !fk.TableNames().Any(p =>
                        p.ToLower().EndsWith("temp")
                        || p.ToLower().EndsWith("old")));
                var tables1 = v.X1.Where(p => constraints.Any(fk => fk.TableNames().Any(name => name == p.TableName)));
                var g = Graph(constraints, tables1);
             
                if (true)
                {
                    List<string> found = new List<string>();
                    var org = g.GetVertex("organization");
                    var outv = System.Console.Out;
                    foreach (var vertex in g.Vertices)
                    {
                        if (!vertex.HasIncidentEdgeWith(org) && !new BredthFirstSearch(vertex, org)
                                                                     {maxDepth = 3, outv = outv}.Bfs().Found())
                        {
                            found.Add(vertex.Data);
                        }
                    }
                    System.Console.WriteLine("-----------------");
                    foreach (var value in found)
                    {
                        System.Console.WriteLine(value);
                    }
                }
                if (false)
                {
                    var traversal = new BredthFirstSearch(g.GetVertex("organization"),
                                                          g.GetVertex("customercategory")) {outv = System.Console.Out};
                    traversal.Bfs();
                    //w.Flush();
                    traversal.ConstructPaths();
                }
            }

            if (null != args.FirstOrDefault(a => a == "-writedot"))
            {
                var v = Serialization.Deserialize();

                DotGraphGenerator.GenerateDotFile(v.X1, v.X2.Union(v.X3));
                DotGraphGenerator.WriteDotFile();
            }
            if (null != args.FirstOrDefault(a => a == "-writeneato"))
            {
                var v = Serialization.Deserialize();
                DotGraphGenerator.GenerateDotFile(v.X1, v.X2.Union(v.X3));
                DotGraphGenerator.WriteNeato();
            }
            if (true)
            {
            }
        }

        private static Graph<string> Graph(IEnumerable<ForeignKeyConstraint> constraints, IEnumerable<Table> tables)
        {
            var g = new Graph<string>(false);
            foreach (var tbl in tables)
            {
                g.AddVertex(tbl.TableName.ToLower());
            }

            foreach (var key in constraints)
            {
                var @from = g.GetVertex(key.FromTableName().ToLower());
                var to = g.GetVertex(key.ToTableName().ToLower());
                Edge<string> edge;
                if ((edge = g.GetEdge(@from, to)) == null)
                    edge = g.AddEdge(@from, to, 1);
                edge.Tag = string.Join(", ", key.Columns.Select(c => c.From.ColumnName).ToArray());
            }
            return g;
        }

        private static void FilterIt(out IEnumerable<ForeignKeyConstraint> foreignKeys, out IEnumerable<Table> tables)
        {
            var v = Serialization.Deserialize();
           
            Func<string, bool> filter = p =>
                                      p.ToLower().EndsWith("temp")
                                      || p.ToLower().EndsWith("old")
                                      || p.ToLower().EndsWith("function");
            var constraints = v.X2.Union(v.X3).Where(fk => !fk.TableNames().Any(filter))
                .Where(fk1 =>
                       !fk1.FromTableName().Equals(fk1.ToTableName(), StringComparison.InvariantCultureIgnoreCase));

            foreignKeys = constraints;
            tables = v.X1.Where(p => constraints.Any(fk => fk.TableNames().Any(name => name == p.TableName)));
        }
    }
}