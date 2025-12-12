namespace backend.Models
{
    public class RedeemedReward
    {
        public Guid RedeemedRewardId { get; set; }
        public Guid UserId { get; set; }
        public Guid RewardId { get; set; }
        public DateTime RedeemedDate { get; set; }
        public bool IsUsed { get; set; }
        public string? UsedAt { get; set; }
        public string? RewardStatus { get; set; }
        public string? RewardComment { get; set; }
        public DateTime? UsedDate { get; set; }
        public string CouponCode { get; set; }
        public virtual User User { get; set; }

        public Rewards Reward { get; set; }
        public RewardType RewardType { get; set; }

    }
}
