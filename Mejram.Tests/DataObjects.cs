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

namespace Mejram.Tests
{
    [TestFixture]
    public class DataObjects
    {
        [Test,Ignore("not a good testcase")]
        public void Obj()
        {
            using (var conn = new SqlConnection("server=(local);Integrated Security=SSPI;database=test"))
            {
                var tables = new DataBaseObjects(conn, new ITableFilter[] {},
                                                 new ITableFilter[] {});
                using (var filef = File.Open("out.txt", FileMode.Create))
                using (var file = new StreamWriter(filef))
                {
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
    }
}