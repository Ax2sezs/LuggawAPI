namespace backend.Models
{
    public class EarnPointRequest
    {
        public string PhoneNumber { get; set; }
        public int Points { get; set; }
        public string? Description { get; set; }
    }
}
