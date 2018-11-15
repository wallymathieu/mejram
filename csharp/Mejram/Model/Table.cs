using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mejram.Model
{
    [Serializable]
    public class Table
    {
        /// <summary>
        /// pg_class.relname
        /// </summary>
        public readonly string TableName;

        private IList<Column> _attributes =
            new List<Column>();

        public IList<Column> Columns
        {
            get { return _attributes; }
        }

        public PrimaryKeyConstraint PrimaryKey;

        public Table(string tableName)
        {
            TableName = tableName;
        }
    }
}
