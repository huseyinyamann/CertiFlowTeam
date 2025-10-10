using System.ComponentModel.DataAnnotations;

namespace CertiFlowTeam.Enums
{
    public enum UserStatus
    {
        [Display(Name = "Aktif")]
        Active = 1,

        [Display(Name = "Pasif")]
        Inactive = 2,

        [Display(Name = "Beklemede")]
        Pending = 3,

        [Display(Name = "Bloke")]
        Blocked = 4
    }
}
