using System.Data.Common;
using System.IO;
using System.Linq;
using Mejram.Models;
using Newtonsoft.Json;

namespace Mejram.Infrastructure
{
    internal class Serialization(string tablesFileName)
    {
        private readonly string _tablesFileName = tablesFileName;

        public void Serialize (DbConnection conn)
        {
            using var fs = File.Open(_tablesFileName, FileMode.Create);
            using var txtWriter = new StreamWriter(fs);
            txtWriter.Write(JsonConvert.SerializeObject(Sql.Tables(conn).ToArray(), Formatting.Indented));
            txtWriter.Flush();
        }
        public Table[] Deserialize ()
        {
            using var fs = File.OpenRead(_tablesFileName);
            using var reader = new StreamReader(fs);
            return JsonConvert.DeserializeObject<Table[]>(reader.ReadToEnd());
        }
    }
}
