using CertiFlowTeam.Enums;
using CertiFlowTeam.Models;
using System.Security.Cryptography;
using System.Text;

namespace CertiFlowTeam.Helpers
{
    public class UserHelper
    {
        private readonly SqlExecutor _sqlExecutor;

        public UserHelper(IConfiguration configuration)
        {
            _sqlExecutor = new SqlExecutor(configuration);
        }

        #region Create User

        public async Task<ServiceResult<int>> CreateUserAsync(UserRegisterModel model)
        {
            try
            {
                var checkSql = @"SELECT COUNT(*) FROM Users
                                WHERE Email = @Email AND IsDeleted = 0";

                var checkParams = new Dictionary<string, object>
                {
                    { "@Email", model.Email }
                };

                var checkResult = await _sqlExecutor.ExecuteScalarAsync<int>(checkSql, checkParams);

                if (checkResult.Success && checkResult.Data > 0)
                {
                    return ServiceResult<int>.ErrorResult("Bu email adresi ile kayıtlı bir kullanıcı zaten mevcut");
                }

                var hashedPassword = HashPassword(model.Password);

                var sql = @"INSERT INTO Users
                           (FullName, Email, Password, Role, CompanyId, Phone, IsActive, CreatedDate, IsDeleted)
                           OUTPUT INSERTED.Id
                           VALUES
                           (@FullName, @Email, @Password, @Role, @CompanyId, @Phone, 1, GETDATE(), 0)";

                var parameters = new Dictionary<string, object>
                {
                    { "@FullName", model.FullName },
                    { "@Email", model.Email },
                    { "@Password", hashedPassword },
                    { "@Role", (int)Role.User },
                    { "@CompanyId", model.CompanyId },
                    { "@Phone", model.Phone ?? string.Empty }
                };

                var result = await _sqlExecutor.ExecuteScalarAsync<int>(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult<int>.SuccessResult(result.Data, "Kullanıcı başarıyla kaydedildi");
                }

                return ServiceResult<int>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.ErrorResult($"Kullanıcı kaydı sırasında hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Login

        public async Task<ServiceResult<UserSessionModel>> LoginAsync(UserLoginModel model)
        {
            try
            {
                var hashedPassword = HashPassword(model.Password);

                var sql = @"SELECT u.Id, u.FullName, u.Email, u.Role, u.CompanyId, c.CompanyName
                           FROM Users u
                           LEFT JOIN Companies c ON u.CompanyId = c.Id
                           WHERE u.Email = @Email
                           AND u.Password = @Password
                           AND u.IsActive = 1
                           AND u.IsDeleted = 0";

                var parameters = new Dictionary<string, object>
                {
                    { "@Email", model.Email },
                    { "@Password", hashedPassword }
                };

                var result = await _sqlExecutor.ExecuteSingleAsync<UserSessionModel>(sql, parameters);

                if (result.Success && result.Data != null && result.Data.UserId > 0)
                {
                    await UpdateLastLoginDateAsync(result.Data.UserId);
                    return ServiceResult<UserSessionModel>.SuccessResult(result.Data, "Giriş başarılı");
                }

                return ServiceResult<UserSessionModel>.ErrorResult("Email veya şifre hatalı");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserSessionModel>.ErrorResult($"Giriş sırasında hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Update Last Login Date

        private async Task<ServiceResult> UpdateLastLoginDateAsync(int userId)
        {
            try
            {
                var sql = @"UPDATE Users SET LastLoginDate = GETDATE() WHERE Id = @UserId";

                var parameters = new Dictionary<string, object>
                {
                    { "@UserId", userId }
                };

                return await _sqlExecutor.ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Son giriş tarihi güncellenirken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Get User By Id

        public async Task<ServiceResult<UserModel>> GetUserByIdAsync(int userId)
        {
            try
            {
                var sql = @"SELECT u.*, c.CompanyName
                           FROM Users u
                           LEFT JOIN Companies c ON u.CompanyId = c.Id
                           WHERE u.Id = @UserId AND u.IsDeleted = 0";

                var parameters = new Dictionary<string, object>
                {
                    { "@UserId", userId }
                };

                var result = await _sqlExecutor.ExecuteSingleAsync<UserModel>(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult<UserModel>.SuccessResult(result.Data);
                }

                return ServiceResult<UserModel>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserModel>.ErrorResult($"Kullanıcı bilgisi alınırken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Get User By Email

        public async Task<ServiceResult<UserModel>> GetUserByEmailAsync(string email)
        {
            try
            {
                var sql = @"SELECT u.*, c.CompanyName
                           FROM Users u
                           LEFT JOIN Companies c ON u.CompanyId = c.Id
                           WHERE u.Email = @Email AND u.IsDeleted = 0";

                var parameters = new Dictionary<string, object>
                {
                    { "@Email", email }
                };

                var result = await _sqlExecutor.ExecuteSingleAsync<UserModel>(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult<UserModel>.SuccessResult(result.Data);
                }

                return ServiceResult<UserModel>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserModel>.ErrorResult($"Kullanıcı bilgisi alınırken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Get All Users

        public async Task<ServiceResult<List<UserModel>>> GetAllUsersAsync()
        {
            try
            {
                var sql = @"SELECT u.*, c.CompanyName
                           FROM Users u
                           LEFT JOIN Companies c ON u.CompanyId = c.Id
                           WHERE u.IsDeleted = 0
                           ORDER BY u.FullName";

                var result = await _sqlExecutor.ExecuteReaderAsync<UserModel>(sql);

                if (result.Success)
                {
                    return ServiceResult<List<UserModel>>.SuccessResult(result.Data);
                }

                return ServiceResult<List<UserModel>>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<UserModel>>.ErrorResult($"Kullanıcı listesi alınırken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Get Users By Company

        public async Task<ServiceResult<List<UserModel>>> GetUsersByCompanyAsync(int companyId)
        {
            try
            {
                var sql = @"SELECT u.*, c.CompanyName
                           FROM Users u
                           LEFT JOIN Companies c ON u.CompanyId = c.Id
                           WHERE u.CompanyId = @CompanyId AND u.IsDeleted = 0
                           ORDER BY u.FullName";

                var parameters = new Dictionary<string, object>
                {
                    { "@CompanyId", companyId }
                };

                var result = await _sqlExecutor.ExecuteReaderAsync<UserModel>(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult<List<UserModel>>.SuccessResult(result.Data);
                }

                return ServiceResult<List<UserModel>>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<UserModel>>.ErrorResult($"Firma kullanıcıları alınırken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Update User

        public async Task<ServiceResult> UpdateUserAsync(UserModel model)
        {
            try
            {
                var sql = @"UPDATE Users SET
                           FullName = @FullName,
                           Email = @Email,
                           Role = @Role,
                           Phone = @Phone,
                           IsActive = @IsActive,
                           UpdatedDate = GETDATE()
                           WHERE Id = @Id AND IsDeleted = 0";

                var parameters = new Dictionary<string, object>
                {
                    { "@Id", model.Id },
                    { "@FullName", model.FullName },
                    { "@Email", model.Email },
                    { "@Role", (int)model.Role },
                    { "@Phone", model.Phone ?? string.Empty },
                    { "@IsActive", model.IsActive }
                };

                var result = await _sqlExecutor.ExecuteNonQueryAsync(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult.SuccessResult("Kullanıcı bilgileri güncellendi");
                }

                return ServiceResult.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Kullanıcı güncellenirken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Change Password

        public async Task<ServiceResult> ChangePasswordAsync(int userId, string newPassword)
        {
            try
            {
                var hashedPassword = HashPassword(newPassword);

                var sql = @"UPDATE Users SET
                           Password = @Password,
                           UpdatedDate = GETDATE()
                           WHERE Id = @UserId AND IsDeleted = 0";

                var parameters = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@Password", hashedPassword }
                };

                var result = await _sqlExecutor.ExecuteNonQueryAsync(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult.SuccessResult("Şifre başarıyla değiştirildi");
                }

                return ServiceResult.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Şifre değiştirilirken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Hash Password

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        #endregion
    }
}
