using CertiFlowTeam.Enums;
using CertiFlowTeam.Models;

namespace CertiFlowTeam.Helpers
{
    public class DocumentHelper
    {
        private readonly SqlExecutor _sqlExecutor;

        public DocumentHelper(IConfiguration configuration)
        {
            _sqlExecutor = new SqlExecutor(configuration);
        }

        #region Create Document

        public async Task<ServiceResult<int>> CreateDocumentAsync(DocumentModel model)
        {
            try
            {
                var sql = @"INSERT INTO Documents
                           (DocumentName, DocumentType, DocumentNumber, FilePath, FileSize, Description,
                            ApprovalStatus, UploadedByUserId, AssignedToUserId, CompanyId, CreatedDate, IsDeleted)
                           OUTPUT INSERTED.Id
                           VALUES
                           (@DocumentName, @DocumentType, @DocumentNumber, @FilePath, @FileSize, @Description,
                            @ApprovalStatus, @UploadedByUserId, @AssignedToUserId, @CompanyId, GETDATE(), 0)";

                var parameters = new Dictionary<string, object>
                {
                    { "@DocumentName", model.DocumentName },
                    { "@DocumentType", model.DocumentType ?? string.Empty },
                    { "@DocumentNumber", model.DocumentNumber ?? string.Empty },
                    { "@FilePath", model.FilePath },
                    { "@FileSize", model.FileSize ?? 0 },
                    { "@Description", model.Description ?? string.Empty },
                    { "@ApprovalStatus", (int)model.ApprovalStatus },
                    { "@UploadedByUserId", model.UploadedByUserId },
                    { "@AssignedToUserId", model.AssignedToUserId.HasValue ? (object)model.AssignedToUserId.Value : DBNull.Value },
                    { "@CompanyId", model.CompanyId.HasValue ? (object)model.CompanyId.Value : DBNull.Value }
                };

                var result = await _sqlExecutor.ExecuteScalarAsync<int>(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult<int>.SuccessResult(result.Data, "Doküman başarıyla yüklendi");
                }

                return ServiceResult<int>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.ErrorResult($"Doküman kaydı sırasında hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Get Document By Id

        public async Task<ServiceResult<DocumentModel>> GetDocumentByIdAsync(int documentId)
        {
            try
            {
                var sql = @"SELECT
                               d.*,
                               u1.FullName AS UploadedByUserName,
                               u2.FullName AS AssignedToUserName,
                               u3.FullName AS ApprovedByUserName,
                               c.CompanyName
                           FROM Documents d
                           LEFT JOIN Users u1 ON d.UploadedByUserId = u1.Id
                           LEFT JOIN Users u2 ON d.AssignedToUserId = u2.Id
                           LEFT JOIN Users u3 ON d.ApprovedByUserId = u3.Id
                           LEFT JOIN Companies c ON d.CompanyId = c.Id
                           WHERE d.Id = @DocumentId AND d.IsDeleted = 0";

                var parameters = new Dictionary<string, object>
                {
                    { "@DocumentId", documentId }
                };

                var result = await _sqlExecutor.ExecuteSingleAsync<DocumentModel>(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult<DocumentModel>.SuccessResult(result.Data);
                }

                return ServiceResult<DocumentModel>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<DocumentModel>.ErrorResult($"Doküman bilgisi alınırken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Get All Documents

        public async Task<ServiceResult<List<DocumentModel>>> GetAllDocumentsAsync()
        {
            try
            {
                var sql = @"SELECT
                               d.*,
                               u1.FullName AS UploadedByUserName,
                               u2.FullName AS AssignedToUserName,
                               u3.FullName AS ApprovedByUserName,
                               c.CompanyName
                           FROM Documents d
                           LEFT JOIN Users u1 ON d.UploadedByUserId = u1.Id
                           LEFT JOIN Users u2 ON d.AssignedToUserId = u2.Id
                           LEFT JOIN Users u3 ON d.ApprovedByUserId = u3.Id
                           LEFT JOIN Companies c ON d.CompanyId = c.Id
                           WHERE d.IsDeleted = 0
                           ORDER BY d.CreatedDate DESC";

                var result = await _sqlExecutor.ExecuteReaderAsync<DocumentModel>(sql);

                if (result.Success)
                {
                    return ServiceResult<List<DocumentModel>>.SuccessResult(result.Data);
                }

                return ServiceResult<List<DocumentModel>>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<DocumentModel>>.ErrorResult($"Doküman listesi alınırken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Get Documents By Company

        public async Task<ServiceResult<List<DocumentModel>>> GetDocumentsByCompanyAsync(int companyId)
        {
            try
            {
                var sql = @"SELECT
                               d.*,
                               u1.FullName AS UploadedByUserName,
                               u2.FullName AS AssignedToUserName,
                               u3.FullName AS ApprovedByUserName,
                               c.CompanyName
                           FROM Documents d
                           LEFT JOIN Users u1 ON d.UploadedByUserId = u1.Id
                           LEFT JOIN Users u2 ON d.AssignedToUserId = u2.Id
                           LEFT JOIN Users u3 ON d.ApprovedByUserId = u3.Id
                           LEFT JOIN Companies c ON d.CompanyId = c.Id
                           WHERE d.CompanyId = @CompanyId AND d.IsDeleted = 0
                           ORDER BY d.CreatedDate DESC";

                var parameters = new Dictionary<string, object>
                {
                    { "@CompanyId", companyId }
                };

                var result = await _sqlExecutor.ExecuteReaderAsync<DocumentModel>(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult<List<DocumentModel>>.SuccessResult(result.Data);
                }

                return ServiceResult<List<DocumentModel>>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<DocumentModel>>.ErrorResult($"Firma dokümanları alınırken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Get Documents By User

        public async Task<ServiceResult<List<DocumentModel>>> GetDocumentsByUserAsync(int userId)
        {
            try
            {
                var sql = @"SELECT
                               d.*,
                               u1.FullName AS UploadedByUserName,
                               u2.FullName AS AssignedToUserName,
                               u3.FullName AS ApprovedByUserName,
                               c.CompanyName
                           FROM Documents d
                           LEFT JOIN Users u1 ON d.UploadedByUserId = u1.Id
                           LEFT JOIN Users u2 ON d.AssignedToUserId = u2.Id
                           LEFT JOIN Users u3 ON d.ApprovedByUserId = u3.Id
                           LEFT JOIN Companies c ON d.CompanyId = c.Id
                           WHERE (d.UploadedByUserId = @UserId OR d.AssignedToUserId = @UserId)
                           AND d.IsDeleted = 0
                           ORDER BY d.CreatedDate DESC";

                var parameters = new Dictionary<string, object>
                {
                    { "@UserId", userId }
                };

                var result = await _sqlExecutor.ExecuteReaderAsync<DocumentModel>(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult<List<DocumentModel>>.SuccessResult(result.Data);
                }

                return ServiceResult<List<DocumentModel>>.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<DocumentModel>>.ErrorResult($"Kullanıcı dokümanları alınırken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Update Document

        public async Task<ServiceResult> UpdateDocumentAsync(DocumentUpdateModel model)
        {
            try
            {
                var sql = @"UPDATE Documents SET
                           DocumentName = @DocumentName,
                           DocumentType = @DocumentType,
                           DocumentNumber = @DocumentNumber,
                           Description = @Description,
                           AssignedToUserId = @AssignedToUserId,
                           UpdatedDate = GETDATE()
                           WHERE Id = @Id AND IsDeleted = 0";

                var parameters = new Dictionary<string, object>
                {
                    { "@Id", model.Id },
                    { "@DocumentName", model.DocumentName },
                    { "@DocumentType", model.DocumentType ?? string.Empty },
                    { "@DocumentNumber", model.DocumentNumber ?? string.Empty },
                    { "@Description", model.Description ?? string.Empty },
                    { "@AssignedToUserId", model.AssignedToUserId.HasValue ? (object)model.AssignedToUserId.Value : DBNull.Value }
                };

                var result = await _sqlExecutor.ExecuteNonQueryAsync(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult.SuccessResult("Doküman bilgileri güncellendi");
                }

                return ServiceResult.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Doküman güncellenirken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Approve or Reject Document

        public async Task<ServiceResult> ApproveOrRejectDocumentAsync(DocumentApprovalModel model)
        {
            try
            {
                var newStatus = model.IsApproved ? DocumentApprovalStatus.Approved : DocumentApprovalStatus.Rejected;

                var sql = @"UPDATE Documents SET
                           ApprovalStatus = @ApprovalStatus,
                           ApprovedByUserId = @ApprovedByUserId,
                           ApprovalDate = GETDATE(),
                           RejectionReason = @RejectionReason,
                           UpdatedDate = GETDATE()
                           WHERE Id = @DocumentId AND IsDeleted = 0";

                var parameters = new Dictionary<string, object>
                {
                    { "@DocumentId", model.DocumentId },
                    { "@ApprovalStatus", (int)newStatus },
                    { "@ApprovedByUserId", model.ApprovedByUserId },
                    { "@RejectionReason", model.RejectionReason ?? string.Empty }
                };

                var result = await _sqlExecutor.ExecuteNonQueryAsync(sql, parameters);

                if (result.Success)
                {
                    var message = model.IsApproved ? "Doküman onaylandı" : "Doküman reddedildi";
                    return ServiceResult.SuccessResult(message);
                }

                return ServiceResult.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Doküman onay işlemi sırasında hata oluştu: {ex.Message}");
            }
        }

        #endregion

        #region Delete Document

        public async Task<ServiceResult> DeleteDocumentAsync(int documentId)
        {
            try
            {
                var sql = @"UPDATE Documents SET
                           IsDeleted = 1,
                           UpdatedDate = GETDATE()
                           WHERE Id = @DocumentId";

                var parameters = new Dictionary<string, object>
                {
                    { "@DocumentId", documentId }
                };

                var result = await _sqlExecutor.ExecuteNonQueryAsync(sql, parameters);

                if (result.Success)
                {
                    return ServiceResult.SuccessResult("Doküman silindi");
                }

                return ServiceResult.ErrorResult(result.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Doküman silinirken hata oluştu: {ex.Message}");
            }
        }

        #endregion
    }
}
