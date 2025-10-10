using CertiFlowTeam.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CertiFlowTeam.Helpers
{
    public class SqlExecutor
    {
        private readonly string _connectionString;

        public SqlExecutor(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        #region Execute NonQuery

        public async Task<ServiceResult> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        return ServiceResult.SuccessResult("İşlem başarılı", rowsAffected);
                    }
                }
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"SQL çalıştırma hatası: {ex.Message}");
            }
        }

        #endregion

        #region Execute Scalar

        public async Task<ServiceResult<T>> ExecuteScalarAsync<T>(string sql, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        var result = await command.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            return ServiceResult<T>.SuccessResult((T)Convert.ChangeType(result, typeof(T)));
                        }

                        return ServiceResult<T>.SuccessResult(default(T));
                    }
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.ErrorResult($"SQL çalıştırma hatası: {ex.Message}");
            }
        }

        #endregion

        #region Execute Reader

        public async Task<ServiceResult<List<Dictionary<string, object>>>> ExecuteReaderAsync(string sql, Dictionary<string, object> parameters = null)
        {
            try
            {
                var resultList = new List<Dictionary<string, object>>();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var columnName = reader.GetName(i);
                                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    row[columnName] = value;
                                }

                                resultList.Add(row);
                            }
                        }
                    }
                }

                return ServiceResult<List<Dictionary<string, object>>>.SuccessResult(resultList);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Dictionary<string, object>>>.ErrorResult($"SQL çalıştırma hatası: {ex.Message}");
            }
        }

        #endregion

        #region Execute Reader With Mapping

        public async Task<ServiceResult<List<T>>> ExecuteReaderAsync<T>(string sql, Dictionary<string, object> parameters = null) where T : new()
        {
            try
            {
                var resultList = new List<T>();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var obj = new T();
                                var properties = typeof(T).GetProperties();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var columnName = reader.GetName(i);
                                    var property = properties.FirstOrDefault(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                                    if (property != null && !reader.IsDBNull(i))
                                    {
                                        var value = reader.GetValue(i);

                                        if (property.PropertyType.IsEnum)
                                        {
                                            property.SetValue(obj, Enum.ToObject(property.PropertyType, value));
                                        }
                                        else if (property.PropertyType == typeof(bool) && value is byte)
                                        {
                                            property.SetValue(obj, Convert.ToBoolean(value));
                                        }
                                        else if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                                        {
                                            var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                                            property.SetValue(obj, Convert.ChangeType(value, underlyingType));
                                        }
                                        else
                                        {
                                            property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                                        }
                                    }
                                }

                                resultList.Add(obj);
                            }
                        }
                    }
                }

                return ServiceResult<List<T>>.SuccessResult(resultList);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<T>>.ErrorResult($"SQL çalıştırma hatası: {ex.Message}");
            }
        }

        #endregion

        #region Execute Single

        public async Task<ServiceResult<T>> ExecuteSingleAsync<T>(string sql, Dictionary<string, object> parameters = null) where T : new()
        {
            try
            {
                var result = await ExecuteReaderAsync<T>(sql, parameters);

                if (result.Success && result.Data != null && result.Data.Count > 0)
                {
                    return ServiceResult<T>.SuccessResult(result.Data.First());
                }

                return ServiceResult<T>.SuccessResult(default(T));
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.ErrorResult($"SQL çalıştırma hatası: {ex.Message}");
            }
        }

        #endregion

        #region Transaction Support

        public async Task<ServiceResult> ExecuteTransactionAsync(Func<SqlConnection, SqlTransaction, Task<ServiceResult>> transactionWork)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var result = await transactionWork(connection, transaction);

                            if (result.Success)
                            {
                                await transaction.CommitAsync();
                                return ServiceResult.SuccessResult("Transaction başarılı");
                            }
                            else
                            {
                                await transaction.RollbackAsync();
                                return result;
                            }
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            return ServiceResult.ErrorResult($"Transaction hatası: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Bağlantı hatası: {ex.Message}");
            }
        }

        #endregion

        #region Test Connection

        public async Task<ServiceResult> TestConnectionAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return ServiceResult.SuccessResult("Veritabanı bağlantısı başarılı");
                }
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Veritabanı bağlantı hatası: {ex.Message}");
            }
        }

        #endregion
    }
}
