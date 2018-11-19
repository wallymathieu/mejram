using System;
using System.Collections.Generic;

namespace Mejram.Model
{
    [Serializable]
    public class PrimaryKeyConstraint : TableConstraint
    {
        public HashSet<ColumnKey> ConstraintKeys =
            new HashSet<ColumnKey>();

        public PrimaryKeyConstraint(string constraintName, string tableName)
            : base(constraintName, tableName)
        {
        }

        public PrimaryKeyConstraint()
        {
        }
    }
}