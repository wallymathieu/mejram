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