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

namespace Mejram.Console
{
	public class SerializeController
	{
		public SerializeController ()
		{
		}

		public string Tables (string connectionString, string database, string fileName)
		{
			using (var conn = Connection(database,connectionString)) 
			{
				var tables = new DataBaseObjects (conn, new ITableFilter[] {}, new ITableFilter[] {});
				using (FileStream fs = File.Open(fileName, FileMode.Create))
				using (TextWriter txtWriter = new StreamWriter(fs)) {
					txtWriter.Write (JsonConvert.SerializeObject (tables.Tables.Values, Formatting.Indented));
					txtWriter.Flush ();
				}
			}
			return null;
		}
		
		public string ForeignKeys (string connectionString, string database, string fileName)
		{
			using (var conn = Connection(database,connectionString)) 
			{
				var tables = new DataBaseObjects (conn, new ITableFilter[] {}, new ITableFilter[] {});
				using (FileStream fs = File.Open(fileName, FileMode.Create))
				using (TextWriter txtWriter = new StreamWriter(fs)) {
					txtWriter.Write (JsonConvert.SerializeObject (tables.ForeignKeys, Formatting.Indented));
					txtWriter.Flush ();
				}
			}
			return null;
		}
		
		public string TableCount (string connectionString, string database, string fileName)
		{
			using (var conn = Connection(database,connectionString)) 
			{
				var tables = new DataBaseObjects (conn, new ITableFilter[] {}, new ITableFilter[] {});

				using (
                    FileStream fs =
                        File.Open(
                            fileName,
                            FileMode.Create))
				using (TextWriter txtWriter = new StreamWriter(fs)) {
					var tableCounts = from table in tables.Tables
                                      let count = tables.GetTableCount (table.Key)
                                      select new KeyValuePair<string, int> (table.Key, count);
					// .GetProbableForeignKeys().Where(fk => !tables.ForeignKeys.Any(rfk => rfk.Equals(fk)));
					txtWriter.Write (JsonConvert.SerializeObject (tableCounts, Formatting.Indented));
					txtWriter.Flush ();
				}
			}
			return null;
		}

		public string ForeignKeyCount (string connectionString, string database, string fileName)
		{
			using (var conn = Connection(database,connectionString)) 
			{
				var tables = new DataBaseObjects (conn, new ITableFilter[] {}, new ITableFilter[] {});

				using (
                    FileStream fs =
                        File.Open(
                            fileName,
                            FileMode.Create))
				using (TextWriter txtWriter = new StreamWriter(fs)) {
					var fkCounts = from fk in tables.ForeignKeys//.Union(probfk)
                                   let count = tables.GetKeyWeight (fk)
                                   select
                                       new KeyValuePair<ForeignKeyConstraint, int> (fk, count);
					// .GetProbableForeignKeys().Where(fk => !tables.ForeignKeys.Any(rfk => rfk.Equals(fk)));
					txtWriter.Write (JsonConvert.SerializeObject (fkCounts, Formatting.Indented));
					txtWriter.Flush ();
				}
			}
			return null;
		}
		private DbConnection Connection(string database, string connstr)
		{
			switch (database.ToLower()) {
			case "npgsql":
			case "postgres":
			{
				var pconn = new NpgsqlConnection(connstr);
				pconn.Open();
				return pconn;
			}
			case "sqlserver":
			case "sql":
			case null:
			case "":
			{
				return new SqlConnection(connstr);
			}
			default:
				throw new Exception("Unknown db");
			}			
		}

	}

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

	public class DotGraphController
	{
		private readonly Serialization _serialization;
		private readonly DotGraphGenerator _dotGraphGenerator;

		public DotGraphController ()
		{
			_serialization = new Serialization ();
			_dotGraphGenerator = new DotGraphGenerator ();
		}

		public void WriteDot ()
		{
			var v = _serialization.Deserialize ();
			var probableForeignKeyConstraints = new PropableForeignKeyAnalysis ().GetProbableForeignKeys (v.Tables);
			_dotGraphGenerator.GenerateDotFile (v.Tables, v.ForeignKeyConstraints.Union (probableForeignKeyConstraints));
			_dotGraphGenerator.WriteDotFile ();
		}

		public void WriteNeato ()
		{
			var v = _serialization.Deserialize ();
			var probableForeignKeyConstraints = new PropableForeignKeyAnalysis ().GetProbableForeignKeys (v.Tables);
			_dotGraphGenerator.GenerateDotFile (v.Tables, v.ForeignKeyConstraints.Union (probableForeignKeyConstraints));
			_dotGraphGenerator.WriteNeato ();
		}
	}

	internal class Program
	{
		private static void Main (string[] args)
		{
			throw new NotImplementedException ("todo: isop here");
		}
	}
}