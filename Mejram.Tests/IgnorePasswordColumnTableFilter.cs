/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
using System.Collections.Generic;
using Mejram.Model;

namespace Mejram.Tests
{
    public class IgnorePasswordColumnTableFilter : ITableFilter
    {
        public bool InitSQL(string alias, List<string> sqlFilterBuffer, List<object> sqlBindsBuffer)
        {
            sqlFilterBuffer.Add(
                string.Format(
                    @"(lower({0}.column_name) not like '%psw%' AND lower({0}.column_name) not like '%password%')", alias));
            return true;
        }
    }
}