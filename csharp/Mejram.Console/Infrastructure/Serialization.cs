using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mejram.Models;
using Newtonsoft.Json;

namespace Mejram
{
    internal class Serialization(string tablesFileName)
    {
        private readonly string _tablesFileName = tablesFileName;

        public async Task Serialize (DbConnection conn)
        {
            using var fs = File.Open(_tablesFileName, FileMode.Create);
            using var txtWriter = new StreamWriter(fs);
            await txtWriter.WriteAsync(JsonConvert.SerializeObject(Sql.Tables(conn).ToArray(), Formatting.Indented));
            await txtWriter.FlushAsync();
        }
        public async Task<Table[]> Deserialize ()
        {
            using var fs = File.OpenRead(_tablesFileName);
            using var reader = new StreamReader(fs);
            return JsonConvert.DeserializeObject<Table[]>(await reader.ReadToEndAsync());
        }
    }
}
