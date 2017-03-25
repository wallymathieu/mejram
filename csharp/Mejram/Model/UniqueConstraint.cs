/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
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