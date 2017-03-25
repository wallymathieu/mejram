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
