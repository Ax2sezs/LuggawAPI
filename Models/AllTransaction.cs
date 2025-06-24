namespace backend.Models
{
    public class AllTransaction
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public Guid? RewardId { get; set; }

        public string? PhoneNumber { get; set; }
        public string? RewardName { get; set; }
        public int Points { get; set; }
        public string TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
    }
}
