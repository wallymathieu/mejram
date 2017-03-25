using System;
using System.Collections.Generic;
using System.Linq;
using Mejram.Model;
using Mejram.NGenerics;
using System.Data.SqlClient;
using System.IO;
using Newtonsoft.Json;
using System.Data.Common;
using Npgsql;

namespace Mejram
{
	public class GraphController
	{
		public IEnumerable<string> Find (string starttable)
		{
			var v = new Serialization ().Deserialize ();
			var probableForeignKeyConstraints = new PropableForeignKeyAnalysis ().GetProbableForeignKeys (v.Tables);
			var constraints = v.ForeignKeyConstraints.Union (probableForeignKeyConstraints)
                .Where (fk => !fk.TableNames ().Any (p =>
                    p.ToLower ().EndsWith ("temp")
                    || p.ToLower ().EndsWith ("old")));
			var tables1 = v.Tables.Where (p => constraints.Any (fk => fk.TableNames ().Any (name => name == p.TableName)));
			var g = Graph (constraints, tables1);

			if (true) {
				var found = new List<string> ();
				var org = g.GetVertex (starttable);
				var outv = System.Console.Out;
				foreach (var vertex in g.Vertices) {
					if (!vertex.HasIncidentEdgeWith (org) && !new BredthFirstSearch (vertex, org) { maxDepth = 3, outv = outv }.Bfs ().Found ()) {
						found.Add (vertex.Data);
					}
				}
				yield return "-----------------";
				foreach (var value in found) {
					yield return (value);
				}
			}
			if (false) {
				var traversal = new BredthFirstSearch (g.GetVertex (starttable),
                                                      g.GetVertex ("customercategory")) { outv = System.Console.Out };
				traversal.Bfs ();
				//w.Flush();
				traversal.ConstructPaths ();
			}
		}

		private static Graph<string> Graph (IEnumerable<ForeignKeyConstraint> constraints, IEnumerable<Table> tables)
		{
			var g = new Graph<string> (false);
			foreach (var tbl in tables) {
				g.AddVertex (tbl.TableName.ToLower ());
			}

			foreach (var key in constraints) {
				var @from = g.GetVertex (key.FromTableName ().ToLower ());
				var to = g.GetVertex (key.ToTableName ().ToLower ());
				Edge<string> edge;
				if ((edge = g.GetEdge (@from, to)) == null)
					edge = g.AddEdge (@from, to, 1);
				edge.Tag = string.Join (", ", key.Columns.Select (c => c.From.ColumnName).ToArray ());
			}
			return g;
		}
	}
	
}