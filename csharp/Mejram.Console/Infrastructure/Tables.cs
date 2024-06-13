using System.Collections.Generic;
using System.Linq;
using Mejram.Models;

namespace Mejram.Infrastructure
{
    static class Tables
    {
         /// <summary> Tables related by foreign keys </summary>
        public static HashSet<string> RelatedTables(ICollection<Table> tables) => new((
                            from table in tables
                            from tfk in table.ForeignKeys
                            from fkcol in tfk.Columns
                            from tableName in new[] { fkcol.From.TableName, fkcol.To.TableName }
                            select tableName).Distinct());

        public static IEnumerable<(Table Table, Table[] ReferencedByPrimalKey)> GroupedByPrimalKeyReferences(ICollection<Table> tables)
        {
            return from table in tables
				where table.HasPrimalKey()
				from referencedPrimalKey in tables
				where referencedPrimalKey.TableName != table.TableName &&
					referencedPrimalKey.ForeignKeys.Any(fk => fk.Columns.Any(fkcol => fkcol.To.TableName == table.TableName))
				
				group (table,referencedPrimalKey) by table.TableName into g
                select (g.First().table, g.Select(t=>t.referencedPrimalKey).ToArray());
        }
    }
}