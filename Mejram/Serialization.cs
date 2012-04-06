/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Mejram.Model;
using Newtonsoft.Json;
using System.Data.Common;

namespace Mejram
{
	public class Serialization
	{
		public string TablesFileName;
		public string ForeignKeysFileName;
		public string ProbableKeysFileName;
		public string ForeignKeysCountFileName;
		public string TableCountFileName;

		public Serialization ()
		{
			TablesFileName = "outfile.Tables.json.txt";
			ForeignKeysFileName = "outfile.ForeignKeys.json.txt";
		}

		public void Serialize (string connectionString)
		{
			using (var conn = new SqlConnection(connectionString)) {
				Serialize (conn);
			}
		}
		
		public void Serialize (DbConnection conn)
		{
 			var tables = new DataBaseObjects (conn, new ITableFilter[] {}, new ITableFilter[] {});
			using (FileStream fs = File.Open(TablesFileName, FileMode.Create))
			using (TextWriter txtWriter = new StreamWriter(fs)) {
				txtWriter.Write (JsonConvert.SerializeObject (tables.Tables.Values, Formatting.Indented));
				txtWriter.Flush ();
			}
			using (FileStream fs = File.Open(ForeignKeysFileName, FileMode.Create))
			using (TextWriter txtWriter = new StreamWriter(fs)) {
				txtWriter.Write (JsonConvert.SerializeObject (tables.ForeignKeys, Formatting.Indented));
				txtWriter.Flush ();
			}
		}

		public SerializedDatabaseSchema Deserialize ()
		{
			List<Table> tables;
			using (var fs = File.OpenRead(TablesFileName))
			using (var reader = new StreamReader(fs)) {
				tables = (List<Table>)JsonConvert.DeserializeObject (reader.ReadToEnd (), typeof(List<Table>));
			}
			List<ForeignKeyConstraint> fks;
			using (var fs = File.OpenRead(ForeignKeysFileName))
			using (var reader = new StreamReader(fs)) {
				fks =
                    (List<ForeignKeyConstraint>)
                    JsonConvert.DeserializeObject (reader.ReadToEnd (), typeof(List<ForeignKeyConstraint>));
			}
			return new SerializedDatabaseSchema (tables, fks);
		}
	}
}