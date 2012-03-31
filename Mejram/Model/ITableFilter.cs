/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/

using System.Collections.Generic;

namespace Mejram.Model
{
    public interface ITableFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="sqlFilterBuffer">buffer of generated sql filters</param>
        /// <param name="sqlBindsBuffer">buffer of bind variables</param>
        /// <returns>returns true if it adds a filter to the buffer</returns>
        bool InitSQL(string alias, List<string> sqlFilterBuffer, List<object> sqlBindsBuffer);
    }
}