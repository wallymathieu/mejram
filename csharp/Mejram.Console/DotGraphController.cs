namespace Mejram
{
     /// <summary>
    /// Writes graph files
    /// </summary>
	public class DotGraphController
	{
	    /// <summary>
	    /// Writes dot file and send to dot
	    /// </summary>
	    /// <param name="dot">dot tool</param>
	    /// <param name="tablesPath">Tables json file path.</param>
	    public void WriteDot (string dot,
            string tablesPath = "outfile.tables.json")
		{
            var dotGraphGenerator = new DotGraphGenerator();
            var serialization = new Serialization(tablesPath);
		    dotGraphGenerator.GenerateDotFile(serialization.Deserialize());
			dotGraphGenerator.WriteDot(dot);
		}

	    /// <summary>
	    /// Writes dot file and send to neato
	    /// </summary>
	    /// <param name="neato">neato tool</param>
	    /// <param name="tablesPath">Tables json file path.</param>
	    public void WriteNeato (string neato,
            string tablesPath = "outfile.Tables.json")
		{
            var dotGraphGenerator = new DotGraphGenerator();
		    var serialization = new Serialization(tablesPath);
			dotGraphGenerator.GenerateDotFile (serialization.Deserialize());
			dotGraphGenerator.WriteNeato(neato);
		}
	}
	
}
