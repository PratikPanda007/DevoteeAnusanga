using System.Data;
using Microsoft.Data.SqlClient;

namespace DevoteeAnusanga.Helper
{
    public static class DBUtils
    {
        private static string? _connectionString;

        //public DBUtils(IConfiguration configuration)
        //{
        //    _connectionString = configuration.GetConnectionString("DefaultConnection")
        //        ?? throw new Exception("DB Connection string not found");
        //}
        public static void Init(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public static SqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("DBUtils not initialized. Call DBUtils.Init() in Program.cs");

            return new SqlConnection(_connectionString);
        }

        public static async Task<DataTable> ExecuteQueryAsync(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters?.Length > 0)
                cmd.Parameters.AddRange(parameters);

            using var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            await Task.Run(() => adapter.Fill(table));
            return table;
        }

        public static async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters?.Length > 0)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<object?> ExecuteScalarAsync(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters?.Length > 0)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteScalarAsync();
        }
    }
}
