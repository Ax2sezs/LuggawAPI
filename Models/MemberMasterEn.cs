using System;

namespace backend.Models
{
    public class MemberMasterEn
    {
        public Guid C_MB_Id { get; set; }
        public byte[] MB_Name { get; set; }
        public byte[] MB_Lastname { get; set; }
        public DateTime? MB_Birthday { get; set; }
        public byte[] MB_IdCard { get; set; }
        public byte[] MB_Tel { get; set; }
        public byte[] MB_Email { get; set; }
        public string MB_Address { get; set; }
        public Guid? MB_Tumbol_Id { get; set; }
        public Guid? MB_Amphur_Id { get; set; }
        public Guid? MB_Province_Id { get; set; }
        public string MB_Postcode { get; set; }
        public string MB_Password { get; set; }
        public string MB_Stat { get; set; }
        public bool Stat { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool StateCode { get; set; }
        public bool DeletionStateCode { get; set; }
        public Guid? VersionNumber { get; set; }
        public string MB_APIStat { get; set; }
        public DateTime? ForgetReset_Dt { get; set; }
        public string MB_Password_Old { get; set; }
        public string MB_Sex { get; set; }
        public string MB_AgeRang { get; set; }
    }
}
