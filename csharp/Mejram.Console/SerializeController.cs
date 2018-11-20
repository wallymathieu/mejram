using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.IO;
using Newtonsoft.Json;
using System.Data.Common;
using Npgsql;

namespace Mejram
{
    /// <summary>
    /// 
    /// </summary>
	public class SerializeController
	{
        /// <summary>
        /// Serialize tables from database
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="database">npgsql, sqlserver</param>
        /// <param name="tablesPath"></param>
        /// <returns></returns>
		public string Tables (string connectionString, string database, string tablesPath = "outfile.Tables.json")
		{
			using (var conn = Connection(database, connectionString)) 
			{
			    var serialization = new Serialization(tablesPath);
			    serialization.Serialize(conn);
			}
			return "Serialized";
		}

        /// <summary>
        /// Get table counts
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="database">npgsql, sqlserver</param>
        /// <param name="tablesPath"></param>
        /// <returns></returns>
		public string TableCount (string connectionString, string database, string tablesPath = "outfile.Tables.json")
		{
			using (var conn = Connection(database,connectionString)) 
			{
				var tables = Sql.Tables(conn);

				using (var fs = File.Open(tablesPath, FileMode.Create))
				using (TextWriter txtWriter = new StreamWriter(fs)) 
				{
					var tableCounts = from table in tables
                                      let count = Sql.TableCount(table.TableName, conn) 
                                      select new KeyValuePair<string, int> (table.TableName, count);
					txtWriter.Write (JsonConvert.SerializeObject (tableCounts, Formatting.Indented));
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