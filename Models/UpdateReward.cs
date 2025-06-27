namespace backend.Models
{
    public class UpdateReward
    {
        public string? RewardName { get; set; }
        public int? PointsRequired { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CouponCode { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? CategoryId { get; set; }
        public RewardType? RewardType { get; set; }

    }
}
