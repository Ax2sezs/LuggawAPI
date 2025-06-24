using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
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

        // ✅ Navigation Property
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}
