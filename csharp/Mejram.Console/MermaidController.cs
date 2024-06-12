namespace Mejram
{
    /// <summary>
    /// Writes graph files
    /// </summary>
    public class MermaidController
	{
        private readonly MermaidGraphGenerator mermaidGenerator = new ();

	    /// <summary>
	    /// Writes mermaid file and send to mermaid cli tool
	    /// </summary>
	    /// <param name="mmdc">mermaid cli tool</param>
	    /// <param name="tablesPath">Tables json file path.</param>
	    public void Write (string mmdc,
            string tablesPath = "outfile.tables.json")
		{
            var serialization = new Serialization(tablesPath);
		    mermaidGenerator.GenerateFile(serialization.Deserialize());
			mermaidGenerator.Write(mmdc);
		}

	}
	
}
