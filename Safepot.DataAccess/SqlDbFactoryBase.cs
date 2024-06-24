using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.DataAccess
{
    public class SqlDbFactoryBase
    {
        private readonly IConfiguration _config;

        public SqlDbFactoryBase(IConfiguration config)
        {
            _config = config;
        }

        internal string DbConnectionString => _config.GetConnectionString("SqlDataConnection") ?? "";

        //internal SqlConnection DbConnection => new SqlConnection(DbConnectionString);
        internal MySql.Data.MySqlClient.MySqlConnection DbConnection => new MySql.Data.MySqlClient.MySqlConnection(DbConnectionString);

        public virtual async Task<int> DbExecuteAsync(string sql, object[] parameters)
        {
            try
            {
                using (var connection = DbConnection)
                {
                    await connection.OpenAsync();
                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.CommandTimeout = 0;
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            cmd.Parameters.AddWithValue((i + 1).ToString(), parameters[i] ?? DBNull.Value);
                        }
                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public virtual async Task<int> DbExecuteAsync(string sql, Dictionary<string, dynamic> parameters)
        {
            try
            {
                using (var connection = DbConnection)
                {
                    await connection.OpenAsync();
                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.CommandTimeout = 0;
                        foreach (var key in parameters.Keys)
                        {
                            cmd.Parameters.AddWithValue(key, parameters[key] ?? DBNull.Value);
                        }
                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public virtual async Task<int> DbExecuteAsync(string sql)
        {
            try
            {
                using (var connection = DbConnection)
                {
                    await connection.OpenAsync();
                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.CommandTimeout = 0;
                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public virtual async Task<int> DbExecuteClearDataAsync()
        {
            try
            {
                using (var connection = DbConnection)
                {
                    await connection.OpenAsync();
                    using (var cmd = new MySqlCommand("Sp_ClearData", connection))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
