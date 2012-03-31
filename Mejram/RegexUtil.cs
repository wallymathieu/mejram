/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
using System.Text.RegularExpressions;

namespace Mejram
{
    public class RegexUtil
    {
        public static string CapText(Match m)
        {
            // Get the matched string.
            string x = m.ToString();
            // If the first char is lower case...
            if (char.IsLower(x[0]))
            {
                // Capitalize it.
                return char.ToUpper(x[0]) + x.Substring(1, x.Length - 1);
            }
            return x;
        }

        /// <summary>
        /// Removes '_' and capitilize the beginning of any word (where '_' is seen as space)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string SQLToDotnetNamingConvention(string text)
        {
            return
                // Capitilize the beginning of all "word":s where '_' is space.
                Regex.Replace(text, @"[^_]+", CapText)
                    // Remove '_' from the string.
                    .Replace("_", "");
        }

        public static string ReplaceColumnPrefix(string text)
        {
            return Regex.Replace(text, @"^[a-zA-Z]{2,4}_", "");
        }
    }
}