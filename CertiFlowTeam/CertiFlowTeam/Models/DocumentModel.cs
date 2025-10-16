using CertiFlowTeam.Enums;

namespace CertiFlowTeam.Models
{
    public class DocumentModel
    {
        public int Id { get; set; }
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string FilePath { get; set; }
        public long? FileSize { get; set; }
        public string Description { get; set; }
        public DocumentApprovalStatus ApprovalStatus { get; set; }
        public int UploadedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string RejectionReason { get; set; }
        public int? CompanyId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }

        public string UploadedByUserName { get; set; }
        public string AssignedToUserName { get; set; }
        public string ApprovedByUserName { get; set; }
        public string CompanyName { get; set; }
    }

    public class DocumentUploadModel
    {
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string Description { get; set; }
        public int? AssignedToUserId { get; set; }
        public IFormFile File { get; set; }
    }

    public class DocumentUpdateModel
    {
        public int Id { get; set; }
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string Description { get; set; }
        public int? AssignedToUserId { get; set; }
    }

    public class DocumentApprovalModel
    {
        public int DocumentId { get; set; }
        public bool IsApproved { get; set; }
        public string RejectionReason { get; set; }
        public int ApprovedByUserId { get; set; }
    }
}
