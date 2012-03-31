/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
using System;
using System.Diagnostics;

namespace Mejram.Model
{
    [Serializable]
    [DebuggerDisplay("{ConstraintName}")]
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