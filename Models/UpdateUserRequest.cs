namespace backend.Models
{
    public class UpdateUserRequest
    {
        public string? DisplayName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
    }
}
