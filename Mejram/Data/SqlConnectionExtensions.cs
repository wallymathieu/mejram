/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Mejram.Util;

namespace Mejram.Data
{
    public static class SqlConnectionExtensions
    {
        public static int ExecuteNonQuery<T>(this SqlConnection conn, String cmdtxt, T binds)
        {
            using (SqlCommand command = GetCommand(conn, cmdtxt, binds))
            {
                return command.ExecuteNonQuery();
            }
        }

        public static object[] ExecuteSingleRow<T>(this SqlConnection conn, String cmdtxt, T binds)
        {
            using (IDbCommand command = GetCommand(conn, cmdtxt, binds))
            {
                command.CommandTimeout = Int32.MaxValue;
                using (IDataReader dataReader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (dataReader.Read())
                    {
                        //DataTable table = dataReader.GetSchemaTable();
                        var row = new object[dataReader.FieldCount];
                        dataReader.GetValues(row);
                        return row;
                    }
                }
            }
            return new object[] {};
        }

        public static SqlDataRecordExtended ExecuteDataReader<T>(this SqlConnection conn, String cmdtxt, T binds)
        {
            using (SqlCommand command = GetCommand(conn, cmdtxt, binds))
            {
                command.CommandTimeout = Int32.MaxValue;
                try
                {
                    return new SqlDataRecordExtended(command.ExecuteReader(CommandBehavior.Default));
                }
                catch (Exception e)
                {
                    throw new Exception("COMMAND text: " + cmdtxt, e);
                }
            }
        }

        public static DataTable ExecuteDataAdapter<T>(this SqlConnection conn, String cmdtxt, T binds)
        {
            using (SqlCommand command = GetCommand(conn, cmdtxt, binds))
            {
                using (var adapter = new SqlDataAdapter(command))
                {
                    var t = new DataTable();
                    adapter.Fill(t);
                    return t;
                }
            }
        }


        public static SqlCommand GetCommand<T>(SqlConnection conn, String cmdtxt, T binds)
        {
            var enumerable = binds as IEnumerable<Object>;
            if (null != enumerable)
            {
                return GetCommandArr(conn, cmdtxt, enumerable);
            }
            //var stringenumerable = binds as IEnumerable<string>;
            //if (null != stringenumerable)
            //{
            //    return GetCommandArr(conn, cmdtxt, stringenumerable);
            //}
            return GetCommandObj(conn, cmdtxt, binds);
        }

        private static SqlCommand GetCommandArr<T>(SqlConnection conn, string cmdtxt, IEnumerable<T> binds)
        {
            var command = new SqlCommand(cmdtxt, conn) {CommandType = CommandType.Text};
            SqlParameter[] parameters = BindsToSqlParameter(binds.Cast<object>());
            foreach (SqlParameter t in parameters)
            {
                command.Parameters.Add(t);
            }
            return command;
        }

        private static SqlCommand GetCommandObj(SqlConnection conn, string cmdtxt, object binds)
        {
            var command = new SqlCommand(cmdtxt, conn) {CommandType = CommandType.Text};
            SqlParameter[] parameters = BindsToSqlParameter2(ReflectionHelper.PropertiesToDictionary(binds));
            foreach (SqlParameter t in parameters)
            {
                command.Parameters.Add(t);
            }
            return command;
        }


        private static SqlParameter[] BindsToSqlParameter(IEnumerable<Object> binds)
        {
            var parameters = new List<SqlParameter>();

            int count = binds.Count();
            if (count%2 != 0)
                throw new Exception(String.Format("binds.Lenght == {0} !!!binds.Length % 2 != 0 ", count));

            object[] arr = binds.ToArray();
            for (int i = 0; i < arr.Length; i += 2)
            {
                parameters.Add(new SqlParameter((string) arr[i], arr[i + 1]));
            }
            return parameters.ToArray();
        }


        private static SqlParameter[] BindsToSqlParameter2(IEnumerable<KeyValuePair<string, object>> binds)
        {
            return binds.Select(o => new SqlParameter(o.Key, o.Value)).ToArray();
        }
    }
}