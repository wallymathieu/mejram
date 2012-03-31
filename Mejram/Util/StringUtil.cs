using System;
using System.Linq;
using System.Text;

namespace Mejram.Util
{
    public class StringUtil
    {
        public static string Agg(string prefix, params string[] arg)
        {
            var sb = new StringBuilder(arg.Length * 5);
            foreach (string t in arg)
            {
                sb.Append(prefix);
                sb.Append(t);
            }
            return sb.ToString();
        }
    }
}
