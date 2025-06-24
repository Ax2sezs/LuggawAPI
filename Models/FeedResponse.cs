namespace backend.Models
{
    public class FeedResponse
    {
        public Guid FeedId { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
