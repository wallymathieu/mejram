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