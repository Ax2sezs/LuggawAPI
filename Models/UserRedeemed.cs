namespace backend.Models
{
    public class UserRedeemed
    {
        public Guid RedeemedRewardId { get; set; }
        public DateTime RedeemedDate { get; set; }
        public Guid RewardId { get; set; }
        public string RewardName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int PointsRequired { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CouponCode { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedDate { get; set; }
        public RewardType RewardType { get; set; }
    }
}
