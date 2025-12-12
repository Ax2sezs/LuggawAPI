namespace backend.Models
{
    public class EarnPointRequest
    {
        public string PhoneNumber { get; set; }
        public int Points { get; set; }
        public string? Description { get; set; }
        public string? EarnCode { get; set; }
        public string? EarnName { get; set; }
        public string? EarnNameEn { get; set; }
        public string? OrderRef { get; set; }
        public int? RemainPoint { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public string? BranchCode { get; set; }
    }
}
