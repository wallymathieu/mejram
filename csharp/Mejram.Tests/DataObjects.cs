/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Mejram.Model;
using NUnit.Framework;
using Npgsql;
using Mejram.StoredProcedures;

namespace Mejram.Tests
{
    [TestFixture]
    public class DataObjects
    {
        [Test, Ignore("need to script setup of sakila db")]
        public void Obj()
        {
            using (var conn = new NpgsqlConnection("Server=127.0.0.1;Port=5432;Database=sakila;User Id=test;Password=test;"))
            {
                conn.Open();
                var tables = new DataBaseObjects(conn, new ITableFilter[] { },
                                                 new ITableFilter[] { });
                using (var filef = File.Open("out.txt", FileMode.Create))
                using (var file = new StreamWriter(filef))
                {
                    file.WriteLine("tables");
                    file.WriteLine(String.Join(Environment.NewLine,
                                               tables.Tables.Values
                                               .Select(p => p.TableName).ToArray()));

                    file.WriteLine("primal keys");
                    file.WriteLine(String.Join(Environment.NewLine,
                                               new PrimalKeyAnalysis().PrimalPrimaryKeys(tables.Tables.Values, tables.ForeignKeys)
                                               .Select(p => p.Key + ": " + p.Value).ToArray()));
                    var fks = new PropableForeignKeyAnalysis().GetProbableForeignKeys(tables.Tables.Values);
                    file.WriteLine("fks");
                    file.WriteLine(String.Join(Environment.NewLine,
                                               fks.Select(
                                                   fk =>
                                                   string.Format("{0}->{1} : {2}", fk.TableName,
                                                                 fk.ConstraintKeys.First().From.ColumnName,
                                                                 fk.ConstraintKeys.First().To.TableName
                                                       /*fk.OtherTable.TableName*/)).ToArray()));
                }
            }
        }
        [Test]
        public void SerializeInfo()
        {
            using (var conn = new NpgsqlConnection("Server=127.0.0.1;Port=5432;Database=sakila;User Id=test;Password=test;"))
            {
                conn.Open();
                var s = new Serialization();
                s.Serialize(conn);
            }
        }

        [Test]
        public void GetStoredProcedures()
        {
            var storedProceduresForPgSql = new PgSqlServer("Server=127.0.0.1;Port=5432;Database=sakila;User Id=test;Password=test;");
            var r = storedProceduresForPgSql.GetRoutines().ToArray();
            var def = storedProceduresForPgSql.GetRoutineDefinition(r.First(r1 => r1.Name == "film_in_stock"));
            Assert.That(def, Is.Not.Empty);
        }
    }
}
