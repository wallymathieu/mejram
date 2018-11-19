using System;
using System.Collections.Generic;

namespace Mejram.Model
{
    [Serializable]
    public class PrimalKey
    {
        public readonly ColumnKey Attribute;

        /// <summary>
        /// pg_class.relname
        /// </summary>
        public readonly string TableName;

        public HashSet<ColumnKey> Attributes =
            new HashSet<ColumnKey>();

        /// <summary>
        /// The name of the type (class) generated
        /// </summary>
        public string ClassTypeName;

        public PrimalKey(string relName, ColumnKey attr)
        {
            TableName = relName;
            Attribute = attr;
            Attributes.Add(attr);
        }

        public override string ToString()
        {
            return "PrimK " + TableName;
        }
    }
}