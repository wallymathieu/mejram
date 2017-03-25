using Mejram.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mejram.StoredProcedures
{
    public class SqlServer : IStoredProcedures
    {
        private string connectionString;
        public SqlServer(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IEnumerable<Routine> GetRoutines()
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(@"SELECT routine_name FROM INFORMATION_SCHEMA.routines", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.CommandType = CommandType.Text;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new Routine(reader.GetString(0));
                        }
                    }
                }
            }
        }
        
        public string GetRoutineDefinition(Routine routine)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(@"select definition from sys.sql_modules 
where object_id=object_id(@id)
", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.NVarChar).Value = routine.Name;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(0);
                        }
                        return null;
                    }
                }
            }
        }
    }
  
}
