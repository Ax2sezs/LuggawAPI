namespace backend.Models
{
    public class AddMemberRequest
    {
        public string MB_Name { get; set; }
        public string MB_Lastname { get; set; }
        public string MB_Birthday { get; set; }
        public string MB_IdCard { get; set; }
        public string MB_Tel { get; set; }
        public string MB_Email { get; set; }
        public string MB_Address { get; set; }
        public string MB_Tumbol_Id { get; set; }
        public string MB_Amphur_Id { get; set; }
        public string MB_Province_Id { get; set; }
        public string MB_Postcode { get; set; }
        public string MB_Password { get; set; }
        public string CreatedBy { get; set; }
        public string VersionNumber { get; set; }
        public string MB_Sex { get; set; }
        public string MB_AgeRang { get; set; }
    }

}
