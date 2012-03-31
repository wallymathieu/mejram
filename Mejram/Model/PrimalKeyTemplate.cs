/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/

using System;

namespace Mejram.Model
{
    [Serializable]
    public abstract class PrimalKeyTemplate
    {
        public abstract Object Key { get; }

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}