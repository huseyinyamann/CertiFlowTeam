namespace CertiFlowTeam.Models
{
    public class CompanyModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string TaxNumber { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string AuthorizedPerson { get; set; }
        public string AuthorizedPhone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CompanyRegisterModel
    {
        public string CompanyName { get; set; }
        public string TaxNumber { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string AuthorizedPerson { get; set; }
        public string AuthorizedPhone { get; set; }
    }
}
