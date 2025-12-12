namespace backend.Models
{
    public class PointTransaction
    {
        public Guid TransactionId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid? RewardId { get; set; }
        public string? RewardName { get; set; }
        public int Points { get; set; }
        public string TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
        public string? EarnCode { get; set; }
        public string? EarnName { get; set; }
        public string? EarnNameEn { get; set; }
        public string? OrderRef { get; set; }
        public int? RemainPoint { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public string? BranchCode { get; set; }
        public User User { get; set; } // ← ต้องมี

    }
}
