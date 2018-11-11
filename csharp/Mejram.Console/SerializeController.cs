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
	public class SerializeController
	{
		public SerializeController ()
		{
		}

		public string Tables (string connectionString, string database, string fileName)
		{
			using (var conn = Connection(database,connectionString)) 
			{
				var tables = new DataBaseObjects (conn, new ITableFilter[] {}, new ITableFilter[] {}, onWarn: Console.Error.WriteLine);
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
				var tables = new DataBaseObjects (conn, new ITableFilter[] {}, new ITableFilter[] {}, onWarn: Console.Error.WriteLine);
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
				var tables = new DataBaseObjects (conn, new ITableFilter[] {}, new ITableFilter[] {}, onWarn: Console.Error.WriteLine);

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
				var tables = new DataBaseObjects (conn, new ITableFilter[] {}, new ITableFilter[] {}, onWarn: Console.Error.WriteLine);

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
	
}