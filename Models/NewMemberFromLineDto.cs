using System;

namespace backend.Models
{
    public class NewMemberFromLineDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateTime? BirthDate { get; set; }
        public string IdCard { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";

        public Guid TumbolId { get; set; } = Guid.Empty;
        public Guid AmphurId { get; set; } = Guid.Empty;
        public Guid ProvinceId { get; set; } = Guid.Empty;

        public string Postcode { get; set; } = "";
        public string Password { get; set; } = "";

        public Guid CreatedBy { get; set; } = Guid.Empty;
        public Guid UpdatedBy { get; set; } = Guid.Empty;

        public string Gender { get; set; } = "";
        public string AgeRange { get; set; } = "";

        public string EncryptKey { get; set; } = "";
        public string EncryptedPhone { get; set; } = "";

        public string Source { get; set; } = "POS";
        public string ExternalUserId { get; set; } = "";
    }
}
