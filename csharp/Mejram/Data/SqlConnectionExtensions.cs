using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Mejram.Util;

namespace Mejram.Data
{
    public static class SqlConnectionExtensions
    {
        public static int ExecuteNonQuery<T>(this IDbConnection conn, String cmdtxt, T binds)
        {
            using (IDbCommand command = GetCommand(conn, cmdtxt, binds))
            {
                return command.ExecuteNonQuery();
            }
        }

        public static object[] ExecuteSingleRow<T>(this IDbConnection conn, String cmdtxt, T binds)
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

        public static SqlDataRecordExtended ExecuteDataReader<T>(this IDbConnection conn, String cmdtxt, T binds)
        {
            using (IDbCommand command = GetCommand(conn, cmdtxt, binds))
            {
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

        public static IDbCommand GetCommand<T>(IDbConnection conn, String cmdtxt, T binds)
        {
            var enumerable = binds as IEnumerable<Object>;
            if (null != enumerable)
            {
                return GetCommandArr(conn, cmdtxt, enumerable);
            }
            return GetCommandObj(conn, cmdtxt, binds);
        }

        private static IDbCommand GetCommandArr<T>(IDbConnection conn, string cmdtxt, IEnumerable<T> binds)
        {
            var command = conn.CreateCommand();
			command.CommandText = cmdtxt;
			command.CommandType = CommandType.Text;
            IDbDataParameter[] parameters = BindsToSqlParameter(command, binds.Cast<object>());
            foreach (var t in parameters)
            {
                command.Parameters.Add(t);
            }
            return command;
        }

        private static IDbCommand GetCommandObj(IDbConnection conn, string cmdtxt, object binds)
        {
           var command = conn.CreateCommand();
			command.CommandText = cmdtxt;
			command.CommandType = CommandType.Text;
			
            IDbDataParameter[] parameters = BindsToSqlParameter2(command, ReflectionHelper.PropertiesToDictionary(binds));
            foreach (var t in parameters)
            {
                command.Parameters.Add(t);
            }
            return command;
        }


        private static IDbDataParameter[] BindsToSqlParameter(IDbCommand command, IEnumerable<Object> binds)
        {
            var parameters = new List<IDbDataParameter>();

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


        private static IDbDataParameter[] BindsToSqlParameter2(IDbCommand command, IEnumerable<KeyValuePair<string, object>> binds)
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
