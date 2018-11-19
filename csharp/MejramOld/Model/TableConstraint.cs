using System;
using System.Diagnostics;

namespace Mejram.Model
{
    [Serializable]
    public class TableConstraint
    {
        public string ConstraintName;
        public string TableName;

        public TableConstraint()
        {
        }

        protected TableConstraint(string constraintName, string tablename)
        {
            ConstraintName = constraintName;
            TableName = tablename;
        }
    }
}