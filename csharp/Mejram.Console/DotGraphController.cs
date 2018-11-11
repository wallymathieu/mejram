using System;
using System.Collections.Generic;
using System.Linq;
using Mejram.Model;
using Mejram.NGenerics;
using System.Data.SqlClient;
using System.IO;
using Newtonsoft.Json;
using System.Data.Common;
using Npgsql;

namespace Mejram
{
	public class DotGraphController
	{

        public DotGraphController ()
		{
		}
        /// <summary>
        /// Writes dot file
        /// </summary>
        /// <param name="dot">dot tool</param>
        /// <param name="tablesPath">Tables json file path.</param>
        /// <param name="foreignKeysPath">Foreign keys file path.</param>
        public void WriteDot (string dot,string tablesPath = "outfile.Tables.json.txt", string foreignKeysPath = "outfile.ForeignKeys.json.txt")
		{
            var _dotGraphGenerator = new DotGraphGenerator(new Configuration{
                DotExe=dot
            });
            var _serialization = new Serialization(foreignKeysFileName: foreignKeysPath, tablesFileName: tablesPath);
            var v = _serialization.Deserialize ();
			var probableForeignKeyConstraints = new PropableForeignKeyAnalysis ().GetProbableForeignKeys (v.Tables);
			_dotGraphGenerator.GenerateDotFile (v.Tables, v.ForeignKeyConstraints.Union (probableForeignKeyConstraints));
			_dotGraphGenerator.WriteDotFile ();
		}
        /// <summary>
        /// Writes dot file
        /// </summary>
        /// <param name="neato">neato tool</param>
        /// <param name="tablesPath">Tables json file path.</param>
        /// <param name="foreignKeysPath">Foreign keys file path.</param>
        public void WriteNeato (string neato, string tablesPath = "outfile.Tables.json.txt", string foreignKeysPath = "outfile.ForeignKeys.json.txt")
		{
            var _dotGraphGenerator = new DotGraphGenerator(new Configuration{
                NeatoExe= neato
            });
            var _serialization = new Serialization(foreignKeysFileName: foreignKeysPath, tablesFileName: tablesPath);
            var v = _serialization.Deserialize ();
			var probableForeignKeyConstraints = new PropableForeignKeyAnalysis ().GetProbableForeignKeys (v.Tables);
			_dotGraphGenerator.GenerateDotFile (v.Tables, v.ForeignKeyConstraints.Union (probableForeignKeyConstraints));
			_dotGraphGenerator.WriteNeato ();
		}
	}
	
}