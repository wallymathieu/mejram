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
		public string Tables (string connectionString, string database, 
			string tablesPath = "outfile.Tables.json")
		{
            using var conn = Connection(database, connectionString);
            var serialization = new Serialization(tablesPath);
            serialization.Serialize(conn);
            return "Serialized";
		}

        /// <summary>
        /// Get table counts
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="database">npgsql, sqlserver</param>
        /// <param name="tablesCountPath"></param>
        /// <returns></returns>
		public string TableCount (string connectionString, string database, 
			string tablesCountPath = "outfile.Tables.Count.json")
		{
            using var conn = Connection(database, connectionString);
            var tables = Sql.Tables(conn);

            using var fs = File.Open(tablesCountPath, FileMode.Create);
            using var txtWriter = new StreamWriter(fs);
            var tableCounts = from table in tables
                              let count = Sql.TableCount(table.TableName, conn)
                              select new KeyValuePair<string, int?>(table.TableName, count);
            txtWriter.Write(JsonConvert.SerializeObject(tableCounts, Formatting.Indented));
            txtWriter.Flush();
            return null;
		}
	    /// <summary>
	    /// Get foreign key weights
	    /// </summary>
	    /// <param name="connectionString"></param>
	    /// <param name="database">npgsql, sqlserver</param>
	    /// <param name="keyWeightPath"></param>
	    /// <returns></returns>
	    public string KeyWeights (string connectionString, string database,
			string keyWeightPath = "outfile.Tables.KeyWeights.json")
	    {
            using var conn = Connection(database, connectionString);
            var tables = Sql.Tables(conn);
            var map = tables.ToDictionary(t => t.TableName, t => t);

            using var fs = File.Open(keyWeightPath, FileMode.Create);
            using var txtWriter = new StreamWriter(fs);
            var tableCounts = from table in tables
                              from fk in table.ForeignKeys
                              let count = Sql.KeyWeight(fk, map, conn)
                              let key = Tuple.Create(table.TableName, fk.ForeignKeyName)
                              select new KeyValuePair<Tuple<string, string>, int?>(key, count);
            txtWriter.Write(JsonConvert.SerializeObject(tableCounts, Formatting.Indented));
            txtWriter.Flush();
            return null;
	    }

		private static DbConnection Connection(string database, string connstr)
		{
			switch (database.ToLower()) {
			case "pg":
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
                var sconn = new SqlConnection(connstr);
                sconn.Open();
				return sconn;
			}
			default:
				throw new Exception("Unknown db");
			}			
		}
	}	
}
