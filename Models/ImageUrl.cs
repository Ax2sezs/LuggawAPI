using System.Text.Json.Serialization;

namespace backend.Models
{
    public class ImageUrl
    {
        public int Id { get; set; } // Primary key เป็น int identity

        public Guid FeedId { get; set; } // Foreign key

        public string Url { get; set; }
        [JsonIgnore]
        public Feeds Feed { get; set; }
    }
}
