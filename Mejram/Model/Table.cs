/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mejram.Model
{
    [Serializable]
    [DebuggerDisplay("{TableName}, {Oid}, {ClassTypeName}, #pk: {PrimaryKeys.Count}, #fk {ForeignKeys.Count} ")]
    public class Table
    {
        /// <summary>
        /// pg_class.relname
        /// </summary>
        public readonly string TableName;

        private IList<Column> _attributes =
            new List<Column>();

        public IEnumerable<Column> Columns
        {
            get { return Attributes; }
        }


        public PrimaryKeyConstraint PrimaryKey;

        // <summary>
        // return all attributes for this class that are foreign keys
        // </summary>
        // <returns></returns>
        //public Set<Pair<long, short>, Column> GetForeignKeyAttributes()
        //{
        //  Set<Pair<long, short>, Column> forAttr = new Set<Pair<long, short>, Column>();
        //  foreach (ForeignKeyConstraint foreign in ForeignKeys)
        //  {
        //    foreach (Column attr in foreign.ConstraintKeys)
        //    {
        //      forAttr.Add(attr.Key, attr);
        //    }
        //  }
        //  return forAttr;
        //}


        public Table(string tableName)
        {
            TableName = tableName;
        }

        public IList<Column> Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }
    }
}