using Microsoft.AspNetCore.Http;

namespace backend.Models
{
    public class CreateRewardRequest
    {
        public string RewardName { get; set; }
        public int PointsRequired { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IFormFile? Image { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
        public RewardType RewardType { get; set; }
        public string DiscountMax { get; set; }
        public string DiscountMin { get; set; }
        public string DiscountPercent { get; set; }
        public string DiscountType { get; set; }

    }
}
