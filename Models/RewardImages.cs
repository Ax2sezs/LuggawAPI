using System.Text.Json.Serialization;

namespace backend.Models
{
    public class RewardImages
    {
        public Guid ImageId { get; set; }
        public Guid RewardId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public Rewards Reward { get; set; }
    }

}
