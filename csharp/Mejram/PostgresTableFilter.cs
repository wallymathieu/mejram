using System;
using Mejram.Model;

namespace Mejram
{
	public class PostgresTableFilter:ITableFilter
	{
		public bool InitSQL (string alias, System.Collections.Generic.List<string> sqlFilterBuffer, System.Collections.Generic.List<object> sqlBindsBuffer)
		{
			sqlFilterBuffer.Add(String.Format("{0}.TABLE_SCHEMA <> 'pg_catalog' AND {0}.TABLE_SCHEMA <> 'information_schema'",alias));
			return true;
		}
	}
}

