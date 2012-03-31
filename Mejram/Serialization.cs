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
        public static string GetProbableKeysFileName()
        {
            return Path.Combine(ConfigurationManager.AppSettings["temp"], "outfile.ProbableForeignKeys.json.txt");
        }

        public static string GetForeignKeysFileName()
        {
            return Path.Combine(ConfigurationManager.AppSettings["temp"], "outfile.ForeignKeys.json.txt");
        }

        public static string GetTablesFileName()
        {
            return Path.Combine(ConfigurationManager.AppSettings["temp"], "outfile.Tables.json.txt");
        }

        public static void Serialize()
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["database"].ConnectionString))
            {
                var tables = new DataBaseObjects(conn, new ITableFilter[] {}, new ITableFilter[] {});
                using (FileStream fs = File.Open(GetTablesFileName(), FileMode.Create))
                using (TextWriter txtWriter = new StreamWriter(fs))
                {
                    txtWriter.Write(JsonConvert.SerializeObject(tables.Tables.Values, Formatting.Indented));
                    txtWriter.Flush();
                }
                using (FileStream fs = File.Open(GetForeignKeysFileName(), FileMode.Create))
                using (TextWriter txtWriter = new StreamWriter(fs))
                {
                    txtWriter.Write(JsonConvert.SerializeObject(tables.ForeignKeys, Formatting.Indented));
                    txtWriter.Flush();
                }
                var probfk = tables.GetProbableForeignKeys().Where(fk => !tables.ForeignKeys.Any(rfk => rfk.Equals(fk)));
                using (FileStream fs = File.Open(GetProbableKeysFileName(), FileMode.Create))
                using (TextWriter txtWriter = new StreamWriter(fs))
                {
                    txtWriter.Write(JsonConvert.SerializeObject(probfk, Formatting.Indented));
                    txtWriter.Flush();
                }
                using (
                    FileStream fs =
                        File.Open(
                            Path.Combine(ConfigurationManager.AppSettings["temp"], "outfile.table.count.json.txt"),
                            FileMode.Create))
                using (TextWriter txtWriter = new StreamWriter(fs))
                {
                    var tableCounts = from table in tables.Tables
                                      let count = tables.GetTableCount(table.Key)
                                      select new System.Collections.Generic.KeyValuePair<string, int>(table.Key, count);
                    // .GetProbableForeignKeys().Where(fk => !tables.ForeignKeys.Any(rfk => rfk.Equals(fk)));
                    txtWriter.Write(JsonConvert.SerializeObject(tableCounts, Formatting.Indented));
                    txtWriter.Flush();
                }
                using (
                    FileStream fs =
                        File.Open(
                            Path.Combine(ConfigurationManager.AppSettings["temp"], "outfile.table.fk.count.json.txt"),
                            FileMode.Create))
                using (TextWriter txtWriter = new StreamWriter(fs))
                {
                    var fkCounts = from fk in tables.ForeignKeys.Union(probfk)
                                   let count = tables.GetKeyWeight(fk)
                                   select
                                       new System.Collections.Generic.KeyValuePair<ForeignKeyConstraint, int>(fk, count);
                    // .GetProbableForeignKeys().Where(fk => !tables.ForeignKeys.Any(rfk => rfk.Equals(fk)));
                    txtWriter.Write(JsonConvert.SerializeObject(fkCounts, Formatting.Indented));
                    txtWriter.Flush();
                }
            }
        }

        public static SerializedDatabaseSchema Deserialize()
        {
            List<Table> tables;
            using (var fs = File.OpenRead(GetTablesFileName()))
            using (var reader = new StreamReader(fs))
            {
                tables = (List<Table>) JsonConvert.DeserializeObject(reader.ReadToEnd(), typeof (List<Table>));
            }
            List<ForeignKeyConstraint> fks;
            using (var fs = File.OpenRead(GetForeignKeysFileName()))
            using (var reader = new StreamReader(fs))
            {
                fks =
                    (List<ForeignKeyConstraint>)
                    JsonConvert.DeserializeObject(reader.ReadToEnd(), typeof (List<ForeignKeyConstraint>));
            }
            List<ForeignKeyConstraint> propable;
            using (var fs = File.OpenRead(GetProbableKeysFileName()))
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
        private readonly List<Table> _tables;

        public List<Table> X1
        {
            get { return _tables; }
        }

        private readonly List<ForeignKeyConstraint> _foreignKeyConstraints;

        public List<ForeignKeyConstraint> X2
        {
            get { return _foreignKeyConstraints; }
        }

        private readonly List<ForeignKeyConstraint> _propable;

        public List<ForeignKeyConstraint> X3
        {
            get { return _propable; }
        }

        public SerializedDatabaseSchema(List<Table> tables, List<ForeignKeyConstraint> foreignKeyConstraints,
                                        List<ForeignKeyConstraint> propable)
        {
            _tables = tables;
            _foreignKeyConstraints = foreignKeyConstraints;
            _propable = propable;
        }
    }
}