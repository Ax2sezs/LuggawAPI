namespace backend.Models
{
    public class UserLog
    {
        public int LogId { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; }
        public string? OldData { get; set; }
        public string? NewData { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
