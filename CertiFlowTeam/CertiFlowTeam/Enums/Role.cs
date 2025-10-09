using System.ComponentModel.DataAnnotations;

namespace CertiFlowTeam.Enums
{
    public enum Role
    {
        [Display(Name = "Sistem Yöneticisi")]
        Administrator = 1,

        [Display(Name = "Yönetici")]
        Manager = 2,

        [Display(Name = "Onaylayıcı")]
        Approver = 3,

        [Display(Name = "Kullanıcı")]
        User = 4
    }
}
