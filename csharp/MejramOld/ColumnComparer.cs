using System;
using Mejram.Model;
using System.Collections.Generic;

namespace Mejram
{
    public class ColumnComparer : IEqualityComparer<ColumnKey>
    {
        public bool Equals(ColumnKey x, ColumnKey y) =>
            string.Equals(x.TableName, y.TableName, StringComparison.CurrentCultureIgnoreCase)
                                                        && x.ColumnName.Equals(y.ColumnName);

        public int GetHashCode(ColumnKey obj) =>
            obj.TableName.ToLower().GetHashCode() + obj.ColumnName.GetHashCode();
    }
}

