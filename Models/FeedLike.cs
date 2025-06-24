namespace backend.Models
{
    public class FeedLike
    {
        public Guid FeedLikeId { get; set; } = Guid.NewGuid();
        public Guid FeedId { get; set; }
        public Guid UserId { get; set; }
        public bool? IsLike { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime LikedAt { get; set; }

        public Feeds Feed { get; set; }
        public User User { get; set; }
    }
}
