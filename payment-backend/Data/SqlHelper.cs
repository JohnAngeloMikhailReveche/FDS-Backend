using System.Data;
using Microsoft.Data.SqlClient;

namespace PaymentService2.Data;

/// <summary>
/// ADO.NET helper for executing stored procedures
/// </summary>
public class SqlHelper
{
    private readonly string _connectionString;

    public SqlHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<T?> ExecuteScalarAsync<T>(string procedureName, params SqlParameter[] parameters)
    {
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(procedureName, conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddRange(parameters);

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();
        
        if (result == null || result == DBNull.Value)
            return default;
            
        return (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task<int> ExecuteNonQueryAsync(string procedureName, params SqlParameter[] parameters)
    {
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(procedureName, conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddRange(parameters);

        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> ExecuteRawSqlAsync(string sql)
    {
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.CommandType = CommandType.Text;

        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<T>> ExecuteReaderAsync<T>(string procedureName, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters)
    {
        var results = new List<T>();
        
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(procedureName, conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddRange(parameters);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            results.Add(mapper(reader));
        }
        
        return results;
    }

    public async Task<T?> ExecuteReaderSingleAsync<T>(string procedureName, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters)
    {
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(procedureName, conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddRange(parameters);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return mapper(reader);
        }
        
        return default;
    }

    // Helper to safely get values from SqlDataReader
    public static T? GetValue<T>(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
            return default;
        return (T)reader.GetValue(ordinal);
    }

    public static string GetString(SqlDataReader reader, string columnName)
    {
        return GetValue<string>(reader, columnName) ?? string.Empty;
    }

    public static decimal GetDecimal(SqlDataReader reader, string columnName)
    {
        return GetValue<decimal>(reader, columnName);
    }

    public static int GetInt(SqlDataReader reader, string columnName)
    {
        return GetValue<int>(reader, columnName);
    }

    public static DateTime GetDateTime(SqlDataReader reader, string columnName)
    {
        return GetValue<DateTime>(reader, columnName);
    }

    public static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
    {
        return GetValue<DateTime?>(reader, columnName);
    }

    public static bool GetBool(SqlDataReader reader, string columnName)
    {
        return GetValue<bool>(reader, columnName);
    }

    /// <summary>
    /// Check if a column exists in the result set
    /// </summary>
    public static bool HasColumn(SqlDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
