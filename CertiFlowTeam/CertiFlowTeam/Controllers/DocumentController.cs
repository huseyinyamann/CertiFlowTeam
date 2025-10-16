using CertiFlowTeam.Enums;
using CertiFlowTeam.Helpers;
using CertiFlowTeam.Models;
using Microsoft.AspNetCore.Mvc;

namespace CertiFlowTeam.Controllers
{
    public class DocumentController : BaseController
    {
        #region Dependencies

        private readonly IConfiguration _configuration;
        private readonly DocumentHelper _documentHelper;
        private readonly UserHelper _userHelper;
        private readonly IWebHostEnvironment _environment;

        public DocumentController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            _documentHelper = new DocumentHelper(configuration);
            _userHelper = new UserHelper(configuration);
        }

        #endregion

        #region Page Actions

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upload()
        {
            return View();
        }

        public IActionResult Detail(int id)
        {
            ViewBag.DocumentId = id;
            return View();
        }

        #endregion

        #region Ajax Methods

        [HttpPost]
        public async Task<JsonResult> UploadDocument(IFormCollection form)
        {
            try
            {
                var userSession = GetUserSession();
                if (userSession == null)
                {
                    return Json(new { success = false, message = "Oturum bulunamadı" });
                }

                var file = form.Files.GetFile("file");

                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Lütfen bir dosya seçin" });
                }

                var documentName = form["documentName"].ToString().Trim();
                if (string.IsNullOrWhiteSpace(documentName))
                {
                    return Json(new { success = false, message = "Doküman adı zorunludur" });
                }

                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Geçersiz dosya formatı. İzin verilen formatlar: PDF, DOC, DOCX, XLS, XLSX, JPG, PNG" });
                }

                var maxFileSize = 10 * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    return Json(new { success = false, message = "Dosya boyutu 10MB'dan büyük olamaz" });
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var relativeFilePath = $"/uploads/documents/{uniqueFileName}";

                var documentModel = new DocumentModel
                {
                    DocumentName = documentName,
                    DocumentType = form["documentType"].ToString().Trim(),
                    DocumentNumber = form["documentNumber"].ToString().Trim(),
                    Description = form["description"].ToString().Trim(),
                    FilePath = relativeFilePath,
                    FileSize = file.Length,
                    ApprovalStatus = DocumentApprovalStatus.Pending,
                    UploadedByUserId = userSession.UserId,
                    CompanyId = userSession.CompanyId
                };

                var assignedToUserIdStr = form["assignedToUserId"].ToString();
                if (!string.IsNullOrWhiteSpace(assignedToUserIdStr) && int.TryParse(assignedToUserIdStr, out int assignedToUserId))
                {
                    documentModel.AssignedToUserId = assignedToUserId;
                }

                var result = await _documentHelper.CreateDocumentAsync(documentModel);

                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message, documentId = result.Data });
                }

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Dosya yüklenirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetDocuments(IFormCollection form)
        {
            try
            {
                var userSession = GetUserSession();
                if (userSession == null)
                {
                    return Json(new { success = false, message = "Oturum bulunamadı" });
                }

                ServiceResult<List<DocumentModel>> result;

                var filterType = form["filterType"].ToString();

                if (filterType == "my")
                {
                    result = await _documentHelper.GetDocumentsByUserAsync(userSession.UserId);
                }
                else if (filterType == "company" && userSession.CompanyId.HasValue)
                {
                    result = await _documentHelper.GetDocumentsByCompanyAsync(userSession.CompanyId.Value);
                }
                else
                {
                    if (userSession.CompanyId.HasValue)
                    {
                        result = await _documentHelper.GetDocumentsByCompanyAsync(userSession.CompanyId.Value);
                    }
                    else
                    {
                        result = await _documentHelper.GetAllDocumentsAsync();
                    }
                }

                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Dokümanlar alınırken hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetDocumentDetail(IFormCollection form)
        {
            try
            {
                var documentIdStr = form["documentId"].ToString();
                if (!int.TryParse(documentIdStr, out int documentId))
                {
                    return Json(new { success = false, message = "Geçersiz doküman ID" });
                }

                var result = await _documentHelper.GetDocumentByIdAsync(documentId);

                if (result.Success && result.Data != null)
                {
                    return Json(new { success = true, data = result.Data });
                }

                return Json(new { success = false, message = result.Message ?? "Doküman bulunamadı" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Doküman detayı alınırken hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> UpdateDocument(IFormCollection form)
        {
            try
            {
                var documentIdStr = form["documentId"].ToString();
                if (!int.TryParse(documentIdStr, out int documentId))
                {
                    return Json(new { success = false, message = "Geçersiz doküman ID" });
                }

                var model = new DocumentUpdateModel
                {
                    Id = documentId,
                    DocumentName = form["documentName"].ToString().Trim(),
                    DocumentType = form["documentType"].ToString().Trim(),
                    DocumentNumber = form["documentNumber"].ToString().Trim(),
                    Description = form["description"].ToString().Trim()
                };

                var assignedToUserIdStr = form["assignedToUserId"].ToString();
                if (!string.IsNullOrWhiteSpace(assignedToUserIdStr) && int.TryParse(assignedToUserIdStr, out int assignedToUserId))
                {
                    model.AssignedToUserId = assignedToUserId;
                }

                var result = await _documentHelper.UpdateDocumentAsync(model);

                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Doküman güncellenirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ApproveDocument(IFormCollection form)
        {
            try
            {
                var userSession = GetUserSession();
                if (userSession == null)
                {
                    return Json(new { success = false, message = "Oturum bulunamadı" });
                }

                var documentIdStr = form["documentId"].ToString();
                if (!int.TryParse(documentIdStr, out int documentId))
                {
                    return Json(new { success = false, message = "Geçersiz doküman ID" });
                }

                var model = new DocumentApprovalModel
                {
                    DocumentId = documentId,
                    IsApproved = true,
                    ApprovedByUserId = userSession.UserId
                };

                var result = await _documentHelper.ApproveOrRejectDocumentAsync(model);

                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Doküman onaylanırken hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> RejectDocument(IFormCollection form)
        {
            try
            {
                var userSession = GetUserSession();
                if (userSession == null)
                {
                    return Json(new { success = false, message = "Oturum bulunamadı" });
                }

                var documentIdStr = form["documentId"].ToString();
                if (!int.TryParse(documentIdStr, out int documentId))
                {
                    return Json(new { success = false, message = "Geçersiz doküman ID" });
                }

                var rejectionReason = form["rejectionReason"].ToString().Trim();
                if (string.IsNullOrWhiteSpace(rejectionReason))
                {
                    return Json(new { success = false, message = "Red nedeni zorunludur" });
                }

                var model = new DocumentApprovalModel
                {
                    DocumentId = documentId,
                    IsApproved = false,
                    RejectionReason = rejectionReason,
                    ApprovedByUserId = userSession.UserId
                };

                var result = await _documentHelper.ApproveOrRejectDocumentAsync(model);

                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Doküman reddedilirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteDocument(IFormCollection form)
        {
            try
            {
                var documentIdStr = form["documentId"].ToString();
                if (!int.TryParse(documentIdStr, out int documentId))
                {
                    return Json(new { success = false, message = "Geçersiz doküman ID" });
                }

                var result = await _documentHelper.DeleteDocumentAsync(documentId);

                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Doküman silinirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetCompanyUsers()
        {
            try
            {
                var userSession = GetUserSession();
                if (userSession == null)
                {
                    return Json(new { success = false, message = "Oturum bulunamadı" });
                }

                ServiceResult<List<UserModel>> result;

                if (userSession.CompanyId.HasValue)
                {
                    result = await _userHelper.GetUsersByCompanyAsync(userSession.CompanyId.Value);
                }
                else
                {
                    result = await _userHelper.GetAllUsersAsync();
                }

                if (result.Success)
                {
                    var users = result.Data.Select(u => new
                    {
                        id = u.Id,
                        fullName = u.FullName,
                        email = u.Email
                    }).ToList();

                    return Json(new { success = true, data = users });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Kullanıcılar alınırken hata oluştu: {ex.Message}" });
            }
        }

        #endregion
    }
}
