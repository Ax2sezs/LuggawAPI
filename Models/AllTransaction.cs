namespace backend.Models
{
    public class AllTransaction
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public Guid? RewardId { get; set; }
        public string? CouponCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? RewardName { get; set; }
        public int Points { get; set; }
        public string TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public string? EarnCode { get; set; }
        public string? EarnName { get; set; }
        public string? EarnNameEn { get; set; }
        public string? OrderRef { get; set; }
        public decimal? RemainPoint { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public string? BranchCode { get; set; }
    }
}
