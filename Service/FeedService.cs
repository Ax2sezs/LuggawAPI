using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Service.Interfaces;
using LineLoginBackend.Data;

namespace backend.Services
{
    public class FeedService : IFeedService
    {
        private readonly AppDbContext _context;

        public FeedService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<FeedDto>> GetActiveFeedsAsync(Guid userId, int pageNumber, int pageSize)
        {
            var query = _context.Feeds
                .Where(f => f.IsActive)
                .Include(f => f.ImageUrls)
                .OrderByDescending(f => f.CreatedAt);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var feedIds = items.Select(f => f.FeedId).ToList();

            // นับ LikeCount ของทุก feedId ที่ดึงมา
            var likeCounts = await _context.FeedLikes
                .Where(fl => feedIds.Contains(fl.FeedId) && fl.IsLike == true)
                .GroupBy(fl => fl.FeedId)
                .Select(g => new { FeedId = g.Key, Count = g.Count() })
                .ToListAsync();

            // ดึง FeedLike ของ user นี้ เพื่อเช็คว่า Like หรือยัง
            var userLikes = await _context.FeedLikes
                .Where(fl => feedIds.Contains(fl.FeedId) && fl.UserId == userId && fl.IsLike == true)
                .Select(fl => fl.FeedId)
                .ToListAsync();

            // แปลง entity Feeds -> FeedDto พร้อมใส่ IsLiked
            var dtoItems = items.Select(f =>
            {
                var likeCount = likeCounts.FirstOrDefault(lc => lc.FeedId == f.FeedId)?.Count ?? 0;
                var isLiked = userLikes.Contains(f.FeedId);

                return new FeedDto
                {
                    FeedId = f.FeedId,
                    Title = f.Title,
                    Content = f.Content,
                    ImageUrls = f.ImageUrls,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    IsActive = f.IsActive,
                    LikeCount = likeCount,
                    IsLiked = isLiked
                };
            }).ToList();

            return new PagedResult<FeedDto>
            {
                Items = dtoItems,
                TotalItems = totalItems,
                Page = pageNumber,
                PageSize = pageSize
            };
        }


        public async Task<string> ToggleLikeAsync(Guid feedId, Guid userId)
        {
            var existingLike = await _context.FeedLikes
                .FirstOrDefaultAsync(x => x.FeedId == feedId && x.UserId == userId);

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            if (existingLike != null)
            {
                // 👉 เคยกดแล้ว: toggle สถานะ
                existingLike.IsLike = !(existingLike.IsLike??false);
                existingLike.UpdatedAt = now;

                _context.FeedLikes.Update(existingLike);
                await _context.SaveChangesAsync();

                return (existingLike.IsLike ?? false) ? "Liked" : "Unliked";
            }
            else
            {
                // 👉 ยังไม่เคยกด: สร้างใหม่เป็น Like
                var newLike = new FeedLike
                {
                    FeedId = feedId,
                    UserId = userId,
                    IsLike = true,
                    LikedAt = now
                };

                _context.FeedLikes.Add(newLike);
                await _context.SaveChangesAsync();

                return "Liked";
            }
        }



    }
}
