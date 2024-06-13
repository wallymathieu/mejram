using System;
using System.Collections.Generic;
using System.Linq;
using Mejram.Infrastructure;
using static Mejram.Infrastructure.Tables;
namespace Mejram
{
	/// <summary>
	/// Meta information about tables
	/// </summary>
    public class SubGraphsController
	{
   		/// <summary>
	    /// Subset of tables that are related by foreign keys
	    /// </summary>
	    public IEnumerable<(string, int, string)> GroupByForeignTables (string tablesPath = "outfile.tables.json")
		{
            var serialization = new Serialization(tablesPath);
			var tables = serialization.Deserialize();
			var relatedTable = RelatedTables(tables);
			return
				from tableAndReferencedPrimalKey in GroupedByPrimalKeyReferences(tables.Where(t => relatedTable.Contains(t.TableName)).ToArray())
				let table=tableAndReferencedPrimalKey.Table
				let referencedPrimalKey=tableAndReferencedPrimalKey.ReferencedByPrimalKey
				where referencedPrimalKey.Length > 1
				select (table.TableName, referencedPrimalKey.Length, string.Join(", ",referencedPrimalKey.Select(t => t.TableName).ToArray()));
		}
	}
}
