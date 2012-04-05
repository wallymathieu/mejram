/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
using System;
using Mejram.Model;
using System.Collections.Generic;

namespace Mejram
{
	public class SerializedDatabaseSchema
	{
		public List<Table> Tables { get; set; }
        public List<ForeignKeyConstraint> ForeignKeyConstraints { get; private set; }
      
        public SerializedDatabaseSchema(List<Table> tables, List<ForeignKeyConstraint> foreignKeyConstraints)
        {
            Tables = tables;
            ForeignKeyConstraints = foreignKeyConstraints;
        }
	}
}

