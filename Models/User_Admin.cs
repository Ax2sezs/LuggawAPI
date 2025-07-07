namespace backend.Models
{
    public class User_Admin
    {
        public Guid UserId { get; set; } = Guid.NewGuid();  // เปลี่ยนจาก int เป็น Guid และกำหนด default ให้สร้าง Guid ใหม่
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Role { get; set; }
        public DateTime? LastLogin { get; set; }

    }
}
