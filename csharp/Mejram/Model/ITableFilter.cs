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