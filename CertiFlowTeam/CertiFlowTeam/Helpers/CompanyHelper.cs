using CertiFlowTeam.Models;

namespace CertiFlowTeam.Helpers
{
    public class CompanyHelper
    {
        private readonly SqlExecutor _sqlExecutor;

        public CompanyHelper(IConfiguration configuration)
        {
            _sqlExecutor = new SqlExecutor(configuration);
        }

        #region Create Company

        public async Task<ServiceResult<int>> CreateCompanyAsync(CompanyRegisterModel model)
        {
            try
            {
                var checkSql = @"SELECT COUNT(*) FROM Companies
                                WHERE (TaxNumber = @TaxNumber OR Email = @Email)
                                AND IsDeleted = 0";

                var checkParams = new Dictionary<string, object>
                {
                    { "@TaxNumber", model.TaxNumber },
                    { "@Email", model.Email }
                };

                var checkResult = await _sqlExecutor.ExecuteScalarAsync<int>(checkSql, checkParams);

                if (checkResult.Success && checkResult.Data > 0)
                {
                    return ServiceResult<int>.ErrorResult("Bu vergi numarası veya email ile kayıtlı bir firma zaten mevcut");
                }

                var sql = @"INSERT INTO Companies
                           (CompanyName, TaxNumber, Address, Phone, Email, AuthorizedPerson, AuthorizedPhone, IsActive, CreatedDate, IsDeleted)
                           OUTPUT INSERTED.Id
                           VALUES
                           (@CompanyName, @TaxNumber, @Address, @Phone, @Email, @AuthorizedPerson, @AuthorizedPhone, 1, GETDATE(), 0)";

                var parameters = new Dictionary<string, object>
                {
                    { "@CompanyName", model.CompanyName },
                    { "@TaxNumber", model.TaxNumber },
                    { "@Address", model.Address ?? string.Empty },
                    { "@Phone", model.Phone ?? string.Empty },
                    { "@Email", model.Email },
                    { "@AuthorizedPerson", model.AuthorizedPerson },
                    { "@AuthorizedPhone", model.AuthorizedPhone ?? string.Empty }
                };

                var result = await _sqlExecutor.ExecuteScalarAsync<int>(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult<int>.SuccessResult(result.Data, "Firma başarıyla kaydedildi");
                }

                return ServiceResult<int>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.ErrorResult($"Firma kaydı sırasında hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Get Company By Id

        public async Task<ServiceResult<CompanyModel>> GetCompanyByIdAsync(int companyId)
        {
            try
            {
                var sql = @"SELECT * FROM Companies
                           WHERE Id = @CompanyId AND IsDeleted = 0";

                var parameters = new Dictionary<string, object>
                {
                    { "@CompanyId", companyId }
                };

                var result = await _sqlExecutor.ExecuteSingleAsync<CompanyModel>(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult<CompanyModel>.SuccessResult(result.Data);
                }

                return ServiceResult<CompanyModel>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<CompanyModel>.ErrorResult($"Firma bilgisi alınırken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Get All Companies

        public async Task<ServiceResult<List<CompanyModel>>> GetAllCompaniesAsync()
        {
            try
            {
                var sql = @"SELECT * FROM Companies
                           WHERE IsDeleted = 0
                           ORDER BY CompanyName";

                var result = await _sqlExecutor.ExecuteReaderAsync<CompanyModel>(sql);

                if (result.Success)
                {
                    return ServiceResult<List<CompanyModel>>.SuccessResult(result.Data);
                }

                return ServiceResult<List<CompanyModel>>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<CompanyModel>>.ErrorResult($"Firma listesi alınırken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Check Company Exists

        public async Task<ServiceResult<bool>> CheckCompanyExistsAsync(string taxNumber, string email)
        {
            try
            {
                var sql = @"SELECT COUNT(*) FROM Companies
                           WHERE (TaxNumber = @TaxNumber OR Email = @Email)
                           AND IsDeleted = 0";

                var parameters = new Dictionary<string, object>
                {
                    { "@TaxNumber", taxNumber },
                    { "@Email", email }
                };

                var result = await _sqlExecutor.ExecuteScalarAsync<int>(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult<bool>.SuccessResult(result.Data > 0);
                }

                return ServiceResult<bool>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.ErrorResult($"Firma kontrolü sırasında hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Update Company

        public async Task<ServiceResult> UpdateCompanyAsync(CompanyModel model)
        {
            try
            {
                var sql = @"UPDATE Companies SET
                           CompanyName = @CompanyName,
                           TaxNumber = @TaxNumber,
                           Address = @Address,
                           Phone = @Phone,
                           Email = @Email,
                           AuthorizedPerson = @AuthorizedPerson,
                           AuthorizedPhone = @AuthorizedPhone,
                           IsActive = @IsActive,
                           UpdatedDate = GETDATE()
                           WHERE Id = @Id AND IsDeleted = 0";

                var parameters = new Dictionary<string, object>
                {
                    { "@Id", model.Id },
                    { "@CompanyName", model.CompanyName },
                    { "@TaxNumber", model.TaxNumber },
                    { "@Address", model.Address ?? string.Empty },
                    { "@Phone", model.Phone ?? string.Empty },
                    { "@Email", model.Email },
                    { "@AuthorizedPerson", model.AuthorizedPerson },
                    { "@AuthorizedPhone", model.AuthorizedPhone ?? string.Empty },
                    { "@IsActive", model.IsActive }
                };

                var result = await _sqlExecutor.ExecuteNonQueryAsync(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult.SuccessResult("Firma bilgileri güncellendi");
                }

                return ServiceResult.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Firma güncellenirken hata oluştu: {ex.Message}");
            }
        }

        #endregion
    }
}
