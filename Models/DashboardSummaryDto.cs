namespace backend.Models
{
    public class DashboardSummaryDto
    {
        public MemberSummaryDto Members { get; set; }
        public GenderSummaryDto GenderSummary { get; set; }
        public RewardSummaryDto Rewards { get; set; }
        public PointSummaryDto Points { get; set; }
        public LikeSummaryDto Likes { get; set; }   // เพิ่ม

    }

    public class MemberSummaryDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
    }
    public class GenderSummaryDto
    {
        public int Male { get; set; }
        public int Female { get; set; }
        public int Other { get; set; } // optional, เผื่อมีค่าอื่นๆ หรือไม่ระบุ
    }

    public class TierCountDto
    {
        public string Tier { get; set; }
        public int Count { get; set; }
    }

    public class RewardSummaryDto
    {
        public int TotalRedeemed { get; set; }
        public List<RewardCountDto> Top3Redeemed { get; set; }
    }

    public class RewardCountDto
    {
        public Guid RewardId { get; set; }
        public string RewardName { get; set; }
        public int Count { get; set; }
    }

    public class PointSummaryDto
    {
        public int Earned { get; set; }
        public int Redeemed { get; set; }
    }

    public class LikeSummaryDto
    {
        public int TotalLikes { get; set; }
        public List<FeedLikeCountDto> Top3LikedFeeds { get; set; }
    }

    public class FeedLikeCountDto
    {
        public Guid FeedId { get; set; }
        public string Title { get; set; }
        public int LikeCount { get; set; }
    }

}
