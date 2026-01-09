namespace backend.Models
{
    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();  // เปลี่ยนจาก int เป็น Guid และกำหนด default ให้สร้าง Guid ใหม่
        public string LineUserId { get; set; }
        public string DisplayName { get; set; }
        public string? PictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; } // "male" / "female" / "other"
        public DateTime? UpdateAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public bool? IsCompleted { get; set; }
        public string? MemberNumber { get; set; }
        public bool? IsAdmin { get; set; }
        public bool? IsAllow {get;set;}

        public UserPoints UserPoint { get; set; }


    }
}
