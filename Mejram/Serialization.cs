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

namespace Mejram
{
    public class Serialization
    {
        public string TablesFileName;
        public string ForeignKeysFileName;
        public string ProbableKeysFileName;
        public string ForeignKeysCountFileName;
        public string TableCountFileName;

        public Serialization()
        {
            TablesFileName = "outfile.Tables.json.txt";
            ForeignKeysFileName = "outfile.ForeignKeys.json.txt";
            ProbableKeysFileName = "outfile.ProbableForeignKeys.json.txt";
            ForeignKeysCountFileName = "outfile.table.fk.count.json.txt";
            TableCountFileName = "outfile.table.count.json.txt";
        }

        public void Serialize(string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var tables = new DataBaseObjects(conn, new ITableFilter[] {}, new ITableFilter[] {});
                using (FileStream fs = File.Open(TablesFileName, FileMode.Create))
                using (TextWriter txtWriter = new StreamWriter(fs))
                {
                    txtWriter.Write(JsonConvert.SerializeObject(tables.Tables.Values, Formatting.Indented));
                    txtWriter.Flush();
                }
                using (FileStream fs = File.Open(ForeignKeysFileName, FileMode.Create))
                using (TextWriter txtWriter = new StreamWriter(fs))
                {
                    txtWriter.Write(JsonConvert.SerializeObject(tables.ForeignKeys, Formatting.Indented));
                    txtWriter.Flush();
                }
                var probfk = tables.GetProbableForeignKeys().Where(fk => !tables.ForeignKeys.Any(rfk => rfk.Equals(fk)));
                using (FileStream fs = File.Open(ProbableKeysFileName, FileMode.Create))
                using (TextWriter txtWriter = new StreamWriter(fs))
                {
                    txtWriter.Write(JsonConvert.SerializeObject(probfk, Formatting.Indented));
                    txtWriter.Flush();
                }
                using (
                    FileStream fs =
                        File.Open(
                            TableCountFileName,
                            FileMode.Create))
                using (TextWriter txtWriter = new StreamWriter(fs))
                {
                    var tableCounts = from table in tables.Tables
                                      let count = tables.GetTableCount(table.Key)
                                      select new KeyValuePair<string, int>(table.Key, count);
                    // .GetProbableForeignKeys().Where(fk => !tables.ForeignKeys.Any(rfk => rfk.Equals(fk)));
                    txtWriter.Write(JsonConvert.SerializeObject(tableCounts, Formatting.Indented));
                    txtWriter.Flush();
                }
                using (
                    FileStream fs =
                        File.Open(
                            ForeignKeysCountFileName,
                            FileMode.Create))
                using (TextWriter txtWriter = new StreamWriter(fs))
                {
                    var fkCounts = from fk in tables.ForeignKeys.Union(probfk)
                                   let count = tables.GetKeyWeight(fk)
                                   select
                                       new KeyValuePair<ForeignKeyConstraint, int>(fk, count);
                    // .GetProbableForeignKeys().Where(fk => !tables.ForeignKeys.Any(rfk => rfk.Equals(fk)));
                    txtWriter.Write(JsonConvert.SerializeObject(fkCounts, Formatting.Indented));
                    txtWriter.Flush();
                }
            }
        }

        public SerializedDatabaseSchema Deserialize()
        {
            List<Table> tables;
            using (var fs = File.OpenRead(TablesFileName))
            using (var reader = new StreamReader(fs))
            {
                tables = (List<Table>) JsonConvert.DeserializeObject(reader.ReadToEnd(), typeof (List<Table>));
            }
            List<ForeignKeyConstraint> fks;
            using (var fs = File.OpenRead(ForeignKeysFileName))
            using (var reader = new StreamReader(fs))
            {
                fks =
                    (List<ForeignKeyConstraint>)
                    JsonConvert.DeserializeObject(reader.ReadToEnd(), typeof (List<ForeignKeyConstraint>));
            }
            List<ForeignKeyConstraint> propable;
            using (var fs = File.OpenRead(ProbableKeysFileName))
            using (var reader = new StreamReader(fs))
            {
                propable =
                    (List<ForeignKeyConstraint>)
                    JsonConvert.DeserializeObject(reader.ReadToEnd(), typeof (List<ForeignKeyConstraint>));
            }
            return new SerializedDatabaseSchema(tables, fks, propable);
        }
    }

    public class SerializedDatabaseSchema
    {
        public List<Table> Tables { get; set; }
        public List<ForeignKeyConstraint> ForeignKeyConstraints { get; private set; }
        public List<ForeignKeyConstraint> ProbableForeignKeyConstraints { get; private set; }

        public SerializedDatabaseSchema(List<Table> tables, List<ForeignKeyConstraint> foreignKeyConstraints,
                                        List<ForeignKeyConstraint> propable)
        {
            Tables = tables;
            ForeignKeyConstraints = foreignKeyConstraints;
            ProbableForeignKeyConstraints = propable;
        }
    }
}