using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Data;
using Mejram.Model;


namespace Mejram.StoredProcedures
{
    public class PgSqlServer : IStoredProcedures
    {
        private string connectionString;
        public PgSqlServer(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IEnumerable<Routine> GetRoutines()
        {
            var list = new List<Routine>();
            using (var sqlConnection = new NpgsqlConnection(connectionString))
            {
                using (var cmd = new NpgsqlCommand(@"
SELECT routine_name 
FROM INFORMATION_SCHEMA.routines 
WHERE 
    routine_schema <> 'pg_catalog'  
    and routine_schema <>'information_schema'
", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.CommandType = CommandType.Text;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Routine(reader.GetString(0)));
                        }
                    }
                }
            }
            return list;
        }

        public string GetRoutineDefinition(Routine routine)
        {
            using (var sqlConnection = new NpgsqlConnection(connectionString))
            {
                using (var cmd = new NpgsqlCommand(@"SELECT prosrc FROM pg_proc where proname =  @id
", sqlConnection))
                {
                    sqlConnection.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@id", routine.Name);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var result =reader.IsDBNull(0) ? null : reader.GetString(0);
                            return result;
                        }
                        throw new Exception("Could not find "+routine.Name);
                    }
                }
            }
        }
    }
}
