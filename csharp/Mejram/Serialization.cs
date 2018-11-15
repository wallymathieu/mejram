using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Mejram.Model;
using Newtonsoft.Json;
using System.Data.Common;
using System;

namespace Mejram
{
	public class Serialization
	{
		public string TablesFileName;
		public string ForeignKeysFileName;
		public string ProbableKeysFileName;
		public string ForeignKeysCountFileName;
		public string TableCountFileName;

		public Serialization(string tablesFileName, string foreignKeysFileName)
        {
			TablesFileName = tablesFileName;
			ForeignKeysFileName = foreignKeysFileName;
		}

		public void Serialize (string connectionString)
		{
			using (var conn = new SqlConnection(connectionString)) {
				Serialize (conn);
			}
		}
		
		public void Serialize (DbConnection conn)
		{
            var tables = new DataBaseObjects (conn, new ITableFilter[] {}, new ITableFilter[] {}, onWarn:Console.Error.WriteLine);
			using (var fs = File.Open(TablesFileName, FileMode.Create))
			using (var txtWriter = new StreamWriter(fs)) {
				txtWriter.Write (JsonConvert.SerializeObject (tables.Tables.Values, Formatting.Indented));
				txtWriter.Flush ();
			}
			using (var fs = File.Open(ForeignKeysFileName, FileMode.Create))
			using (var txtWriter = new StreamWriter(fs)) {
				txtWriter.Write (JsonConvert.SerializeObject (tables.ForeignKeys, Formatting.Indented));
				txtWriter.Flush ();
			}
		}

		public SerializedDatabaseSchema Deserialize ()
		{
			List<Table> tables;
			using (var fs = File.OpenRead(TablesFileName))
			using (var reader = new StreamReader(fs)) {
				tables = JsonConvert.DeserializeObject<List<Table>>(reader.ReadToEnd());
			}
			List<ForeignKeyConstraint> fks;
			using (var fs = File.OpenRead(ForeignKeysFileName))
			using (var reader = new StreamReader(fs)) {
				fks = JsonConvert.DeserializeObject<List<ForeignKeyConstraint>>(reader.ReadToEnd());
			}
			return new SerializedDatabaseSchema (tables, fks);
		}
	}
}