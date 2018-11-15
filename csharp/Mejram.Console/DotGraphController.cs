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
     /// <summary>
    /// Writes graph files
    /// </summary>
	public class DotGraphController
	{

        ///
        public DotGraphController ()
		{
		}
        /// <summary>
        /// Writes dot file and send to dot
        /// </summary>
        /// <param name="dot">dot tool</param>
        /// <param name="tablePrefixes"></param>
        /// <param name="keyNames"></param>
        /// <param name="tablesPath">Tables json file path.</param>
        /// <param name="foreignKeysPath">Foreign keys file path.</param>
        public void WriteDot (string dot,
            string[] tablePrefixes=null, string[] keyNames=null,
            string tablesPath = "outfile.Tables.json.txt", string foreignKeysPath = "outfile.ForeignKeys.json.txt")
		{
            var _dotGraphGenerator = new DotGraphGenerator(new Configuration{
                DotExe=dot
            });
            var _serialization = new Serialization(foreignKeysFileName: foreignKeysPath, tablesFileName: tablesPath);
            var v = _serialization.Deserialize ();
			var probableForeignKeyConstraints = new ProbableForeignKeyAnalysis (
			                                        onWarn:err=>Console.Error.WriteLine(err),
                                                    keyNames:keyNames??new string[0], 
                                                    tablePrefixes:tablePrefixes??new string[0])
                                                    .GetProbableForeignKeys (v.Tables);
			_dotGraphGenerator.GenerateDotFile (v.Tables, v.ForeignKeyConstraints.Union (probableForeignKeyConstraints));
			_dotGraphGenerator.WriteDot();
		}
        /// <summary>
        /// Writes dot file and send to neato
        /// </summary>
        /// <param name="neato">neato tool</param>
        /// <param name="tablePrefixes"></param>
        /// <param name="keyNames"></param>
        /// <param name="tablesPath">Tables json file path.</param>
        /// <param name="foreignKeysPath">Foreign keys file path.</param>
        public void WriteNeato (string neato, 
            string[] tablePrefixes=null, string[] keyNames=null,
            string tablesPath = "outfile.Tables.json.txt", string foreignKeysPath = "outfile.ForeignKeys.json.txt")
		{
            var _dotGraphGenerator = new DotGraphGenerator(new Configuration{
                NeatoExe= neato
            });
            var _serialization = new Serialization(foreignKeysFileName: foreignKeysPath, tablesFileName: tablesPath);
            var v = _serialization.Deserialize ();
			var probableForeignKeyConstraints = new ProbableForeignKeyAnalysis (
                                			        onWarn:err=>Console.Error.WriteLine(err),
                                                    keyNames:keyNames??new string[0], 
                                                    tablePrefixes:tablePrefixes??new string[0])
                                                    .GetProbableForeignKeys (v.Tables);
			_dotGraphGenerator.GenerateDotFile (v.Tables, v.ForeignKeyConstraints.Union (probableForeignKeyConstraints));
			_dotGraphGenerator.WriteNeato();
		}
	}
	
}
