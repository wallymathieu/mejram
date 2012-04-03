using System;
using Mejram.Model;
using System.Collections.Generic;

namespace Mejram
{
	public class SerializedDatabaseSchema
	{
		public List<Table> Tables { get; set; }
        public List<ForeignKeyConstraint> ForeignKeyConstraints { get; private set; }
      
        public SerializedDatabaseSchema(List<Table> tables, List<ForeignKeyConstraint> foreignKeyConstraints)
        {
            Tables = tables;
            ForeignKeyConstraints = foreignKeyConstraints;
        }
	}
}

