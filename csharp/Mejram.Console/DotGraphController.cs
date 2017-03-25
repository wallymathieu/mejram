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
		private readonly Serialization _serialization;
		private readonly DotGraphGenerator _dotGraphGenerator;

		public DotGraphController ()
		{
			_serialization = new Serialization ();
			_dotGraphGenerator = new DotGraphGenerator ();
		}

		public void WriteDot ()
		{
			var v = _serialization.Deserialize ();
			var probableForeignKeyConstraints = new PropableForeignKeyAnalysis ().GetProbableForeignKeys (v.Tables);
			_dotGraphGenerator.GenerateDotFile (v.Tables, v.ForeignKeyConstraints.Union (probableForeignKeyConstraints));
			_dotGraphGenerator.WriteDotFile ();
		}

		public void WriteNeato ()
		{
			var v = _serialization.Deserialize ();
			var probableForeignKeyConstraints = new PropableForeignKeyAnalysis ().GetProbableForeignKeys (v.Tables);
			_dotGraphGenerator.GenerateDotFile (v.Tables, v.ForeignKeyConstraints.Union (probableForeignKeyConstraints));
			_dotGraphGenerator.WriteNeato ();
		}
	}
	
}