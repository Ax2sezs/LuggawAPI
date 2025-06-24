namespace backend.Models
{
    public class LineProfile
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string PictureUrl { get; set; }
        public string StatusMessage { get; set; }
    }
}
