namespace backend.Models
{
    public class Feeds
    {
        public Guid FeedId { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public List<ImageUrl> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }

    }

    public class FeedDto
    {
        public Guid FeedId { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public List<ImageUrl> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public int LikeCount { get; set; }
        public bool? IsLiked { get; set; }

    }

}
