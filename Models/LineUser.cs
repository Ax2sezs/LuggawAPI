namespace backend.Models
{
    public class LineUser
    {
        public Guid UserId { get; set; } = Guid.NewGuid();  // เปลี่ยนจาก int เป็น Guid และกำหนด default ให้สร้าง Guid ใหม่
        public string LineUserId { get; set; } // จาก LINE
        public string DisplayName { get; set; }
        public string PictureUrl { get; set; }

        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; } // "male" / "female" / "other"
    }
}
