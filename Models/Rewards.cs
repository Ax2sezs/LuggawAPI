using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public enum RewardType
    {
        General = 0,
        UniqueUse = 1,
        Exclusive = 2,
    }

    public class Rewards
    {
        public Guid RewardId { get; set; }
        public string RewardName { get; set; }
        public int PointsRequired { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ImageUrl { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? CouponCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int CategoryId { get; set; }
        public RewardType RewardType { get; set; }
        public string DiscountMax { get; set; }
        public string DiscountMin { get; set; }
        public string DiscountPercent { get; set; }
        public string DiscountType { get; set; }

        // ✅ Navigation Property
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}
