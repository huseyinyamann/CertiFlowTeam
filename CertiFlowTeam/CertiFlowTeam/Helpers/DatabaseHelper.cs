using CertiFlowTeam.Constants;
using Microsoft.Data.SqlClient;
using System.Text;

namespace CertiFlowTeam.Helpers
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        #region Database Check and Creation

        public async Task<(bool Success, string Message, List<string> Details)> CheckAndCreateTablesAsync()
        {
            var details = new List<string>();
            var sb = new StringBuilder();

            try
            {
                details.Add("✓ Veritabanı bağlantısı kontrol ediliyor...");

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    details.Add("✓ Veritabanı bağlantısı başarılı");

                    foreach (var tableName in SqlConstants.TableCreationOrder)
                    {
                        details.Add($"→ {tableName} tablosu kontrol ediliyor...");

                        bool tableExists = await TableExistsAsync(connection, tableName);

                        if (!tableExists)
                        {
                            details.Add($"  ⚠ {tableName} tablosu bulunamadı, oluşturuluyor...");

                            string createScript = GetCreateScriptForTable(tableName);

                            if (!string.IsNullOrEmpty(createScript))
                            {
                                await ExecuteNonQueryAsync(connection, createScript);
                                details.Add($"  ✓ {tableName} tablosu başarıyla oluşturuldu");
                            }
                            else
                            {
                                details.Add($"  ✗ {tableName} tablosu için create script bulunamadı");
                            }
                        }
                        else
                        {
                            details.Add($"  ✓ {tableName} tablosu mevcut");
                            await CheckAndAddMissingColumnsAsync(connection, tableName, details);
                        }
                    }

                    details.Add("");
                    details.Add("══════════════════════════════════════");
                    details.Add("✓ Tüm tablolar başarıyla kontrol edildi ve oluşturuldu!");
                    details.Add("══════════════════════════════════════");

                    return (true, "Veritabanı kontrol işlemi başarıyla tamamlandı", details);
                }
            }
            catch (Exception ex)
            {
                details.Add("");
                details.Add("══════════════════════════════════════");
                details.Add($"✗ HATA: {ex.Message}");
                details.Add("══════════════════════════════════════");

                return (false, $"Hata: {ex.Message}", details);
            }
        }

        private async Task<bool> TableExistsAsync(SqlConnection connection, string tableName)
        {
            string script = SqlConstants.GetTableExistsScript(tableName);

            using (var command = new SqlCommand(script, connection))
            {
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
        }

        private async Task CheckAndAddMissingColumnsAsync(SqlConnection connection, string tableName, List<string> details)
        {
            if (tableName == SqlConstants.TableNames.Documents)
            {
                if (!await ColumnExistsAsync(connection, tableName, "DocumentType"))
                {
                    details.Add($"    ⚠ DocumentType kolonu eksik, ekleniyor...");
                    await ExecuteNonQueryAsync(connection, SqlConstants.MigrationScripts.AddDocumentTypeToDocuments);
                    details.Add($"    ✓ DocumentType kolonu eklendi");
                }
            }
        }

        private async Task<bool> ColumnExistsAsync(SqlConnection connection, string tableName, string columnName)
        {
            string script = SqlConstants.GetColumnExistsScript(tableName, columnName);

            using (var command = new SqlCommand(script, connection))
            {
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
        }

        private string GetCreateScriptForTable(string tableName)
        {
            return tableName switch
            {
                SqlConstants.TableNames.Companies => SqlConstants.CreateTableScripts.Companies,
                SqlConstants.TableNames.Users => SqlConstants.CreateTableScripts.Users,
                SqlConstants.TableNames.Documents => SqlConstants.CreateTableScripts.Documents,
                SqlConstants.TableNames.DocumentLogs => SqlConstants.CreateTableScripts.DocumentLogs,
                SqlConstants.TableNames.Settings => SqlConstants.CreateTableScripts.Settings,
                _ => string.Empty
            };
        }

        private async Task ExecuteNonQueryAsync(SqlConnection connection, string script)
        {
            var batches = script.Split(new[] { "GO", "go" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;

                using (var command = new SqlCommand(batch.Trim(), connection))
                {
                    command.CommandTimeout = 300;
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        #endregion

        #region Test Connection

        public async Task<(bool Success, string Message)> TestConnectionAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return (true, "Veritabanı bağlantısı başarılı");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Veritabanı bağlantı hatası: {ex.Message}");
            }
        }

        #endregion
    }
}
