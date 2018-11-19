using System;
using System.Collections.Generic;

namespace Mejram.Model
{
    [Serializable]
    public class UniqueConstraint : TableConstraint
    {
        public HashSet<ColumnKey> ConstraintKeys =
            new HashSet<ColumnKey>();

        public UniqueConstraint(string constraintName, string tableName)
            : base(constraintName, tableName)
        {
        }

        public UniqueConstraint()
        {
        }
    }
}