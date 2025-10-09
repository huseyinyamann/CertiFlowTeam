using System.ComponentModel.DataAnnotations;

namespace CertiFlowTeam.Enums
{
    public enum DocumentApprovalStatus
    {
        [Display(Name = "Taslak")]
        Draft = 0,

        [Display(Name = "Onay Bekliyor")]
        Pending = 1,

        [Display(Name = "İnceleniyor")]
        InReview = 2,

        [Display(Name = "Onaylandı")]
        Approved = 3,

        [Display(Name = "Reddedildi")]
        Rejected = 4,

        [Display(Name = "İptal Edildi")]
        Cancelled = 5
    }
}
