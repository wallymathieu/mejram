using System.Threading.Tasks;

namespace Mejram
{
     /// <summary>
    /// Writes graph files
    /// </summary>
	public class DotGraphController
	{
        private readonly DotGraphGenerator dotGraphGenerator = new();
	    /// <summary>
	    /// Writes dot file and send to dot
	    /// </summary>
	    /// <param name="dot">dot tool</param>
	    /// <param name="tablesPath">Tables json file path.</param>
	    public async Task WriteDot (string dot,
            string tablesPath = "outfile.tables.json")
		{
            var serialization = new Serialization(tablesPath);
		    await dotGraphGenerator.GenerateDotFile(await serialization.Deserialize());
			await dotGraphGenerator.WriteDot(dot);
		}

	    /// <summary>
	    /// Writes dot file and send to neato
	    /// </summary>
	    /// <param name="neato">neato tool</param>
	    /// <param name="tablesPath">Tables json file path.</param>
	    public async Task WriteNeato (string neato,
            string tablesPath = "outfile.Tables.json")
		{
		    var serialization = new Serialization(tablesPath);
			await dotGraphGenerator.GenerateDotFile (await serialization.Deserialize());
			await dotGraphGenerator.WriteNeato(neato);
		}
	}
	
}
