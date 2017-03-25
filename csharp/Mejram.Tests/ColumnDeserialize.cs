/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
using Mejram.Model;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Mejram.Tests
{
    [TestFixture]
    public class ColumnDeserialize
    {
        [Test]
        public void Test()
        {
            var v =
                (ForeignKeyColumn)
                JsonConvert.DeserializeObject(
                    @" {
        ""From"": {
          ""TableName"": ""PurchaseOrderSet"",
          ""ColumnName"": ""PurchaseOrderId""
        },
        ""To"": {
          ""TableName"": ""PurchaseOrder"",
          ""ColumnName"": ""Id""
        },
      }",
                    typeof (ForeignKeyColumn));
            Assert.That(v.From, Is.EqualTo(new ColumnKey("PurchaseOrderSet", "PurchaseOrderId")));
            Assert.That(v.To, Is.EqualTo(new ColumnKey("PurchaseOrder", "Id")));
        }
    }
}