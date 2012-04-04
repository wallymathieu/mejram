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
using System.Data.Common;

namespace Mejram.Data
{
    public static class SqlConnectionExtensions
    {
        public static int ExecuteNonQuery<T>(this DbConnection conn, String cmdtxt, T binds)
        {
            using (DbCommand command = GetCommand(conn, cmdtxt, binds))
            {
                return command.ExecuteNonQuery();
            }
        }

        public static object[] ExecuteSingleRow<T>(this DbConnection conn, String cmdtxt, T binds)
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

        public static SqlDataRecordExtended ExecuteDataReader<T>(this DbConnection conn, String cmdtxt, T binds)
        {
            using (DbCommand command = GetCommand(conn, cmdtxt, binds))
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

        public static DbCommand GetCommand<T>(DbConnection conn, String cmdtxt, T binds)
        {
            var enumerable = binds as IEnumerable<Object>;
            if (null != enumerable)
            {
                return GetCommandArr(conn, cmdtxt, enumerable);
            }
            return GetCommandObj(conn, cmdtxt, binds);
        }

        private static DbCommand GetCommandArr<T>(DbConnection conn, string cmdtxt, IEnumerable<T> binds)
        {
            var command = conn.CreateCommand();
			command.CommandText = cmdtxt;
			command.CommandType = CommandType.Text;
            DbParameter[] parameters = BindsToSqlParameter(command, binds.Cast<object>());
            foreach (var t in parameters)
            {
                command.Parameters.Add(t);
            }
            return command;
        }

        private static DbCommand GetCommandObj(DbConnection conn, string cmdtxt, object binds)
        {
           var command = conn.CreateCommand();
			command.CommandText = cmdtxt;
			command.CommandType = CommandType.Text;
			
            DbParameter[] parameters = BindsToSqlParameter2(command, ReflectionHelper.PropertiesToDictionary(binds));
            foreach (var t in parameters)
            {
                command.Parameters.Add(t);
            }
            return command;
        }


        private static DbParameter[] BindsToSqlParameter(DbCommand command, IEnumerable<Object> binds)
        {
            var parameters = new List<DbParameter>();

            int count = binds.Count();
            if (count%2 != 0)
                throw new Exception(String.Format("binds.Lenght == {0} !!!binds.Length % 2 != 0 ", count));

            object[] arr = binds.ToArray();
            for (int i = 0; i < arr.Length; i += 2)
            {
				var p = command.CreateParameter();
				p.ParameterName = (string) arr[i];
				p.Value = arr[i + 1];
                parameters.Add(p);
            }
            return parameters.ToArray();
        }


        private static DbParameter[] BindsToSqlParameter2(DbCommand command, IEnumerable<KeyValuePair<string, object>> binds)
        {
            return binds.Select(o => {
				var p = command.CreateParameter();
				p.ParameterName=o.Key;
				p.Value = o.Value;
				return p;
			}).ToArray();
        }
    }
}