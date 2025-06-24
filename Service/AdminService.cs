
using backend.Service.Interfaces;
using LineLoginBackend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace backend.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly FileSettings _fileSettings;

        public AdminService(AppDbContext context,IWebHostEnvironment environment, IOptions<FileSettings> fileSettings)
        {
            _context = context;
            _environment = environment;
            _fileSettings = fileSettings.Value;
        }
        private async Task<List<string>> SaveImages(List<IFormFile> images)
        {
            var imageUrls = new List<string>();
            Directory.CreateDirectory(_fileSettings.UploadFolder);

            foreach (var image in images)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var fullPath = Path.Combine(_fileSettings.UploadFolder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await image.CopyToAsync(stream);

                imageUrls.Add(Path.Combine(_fileSettings.BaseUrl, fileName).Replace("\\", "/"));
            }

            return imageUrls;
        }

        public async Task<PagedResult<ShowAllUser>> GetAllUsersAsync(
     int page,
     int pageSize,
     string? searchTerm = null,     // กรองโดยชื่อหรือเบอร์โทร
     bool? isActive = null,         // กรองสถานะเปิดใช้งาน
     DateTime? createdAfter = null, // กรองวันที่สมัครหลังจากนี้
     DateTime? createdBefore = null // กรองวันที่สมัครก่อนหน้านี้
 )
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var query = _context.Users
                .Include(u => u.UserPoint)
                .AsNoTracking()
                .AsQueryable();

            // Filter: ค้นหาชื่อ (DisplayName) หรือเบอร์โทร (PhoneNumber) ถ้ามีค่า searchTerm
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string loweredTerm = searchTerm.Trim().ToLower();
                query = query.Where(u =>
                    (u.DisplayName != null && u.DisplayName.ToLower().Contains(loweredTerm))
                    || (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(loweredTerm))
                );
            }

            // Filter: กรองสถานะเปิดใช้งาน
            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            // Filter: กรองวันที่สมัคร (CreatedAt) หลังจากวันที่กำหนด
            if (createdAfter.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= createdAfter.Value);
            }

            // Filter: กรองวันที่สมัครก่อนวันที่กำหนด
            if (createdBefore.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= createdBefore.Value);
            }

            var totalItems = await query.CountAsync();

            var userList = await query
                .OrderBy(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var users = userList.Select(u => new ShowAllUser
            {
                UserId = u.UserId,
                CreatedAt = u.CreatedAt,
                PhoneNumber = u.PhoneNumber,
                BirthDate = u.BirthDate,
                Gender = u.Gender,
                Point = u.UserPoint?.TotalPoints,
                IsActive = u.IsActive,
                Age = u.BirthDate.HasValue
                    ? (now.Year - u.BirthDate.Value.Year - (u.BirthDate.Value.Date > now.Date.AddYears(-(now.Year - u.BirthDate.Value.Year)) ? 1 : 0))
                    : (int?)null
            }).ToList();

            return new PagedResult<ShowAllUser>
            {
                Items = users,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task ToggleUserStatusAsync(ToggleRequest request)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                throw new Exception("User not found");

            user.IsActive = request.IsActive;
            user.UpdateAt = now;

            await _context.SaveChangesAsync();
        }
        private string GenerateCouponCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Category.ToListAsync();
        }
        public async Task<string> GenerateUniqueRewardCodeAsync(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Prefix is required");

            var latestCode = await _context.Rewards
                .Where(r => r.CouponCode.StartsWith(prefix))
                .OrderByDescending(r => r.CouponCode)
                .Select(r => r.CouponCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(latestCode) &&
                int.TryParse(latestCode.Substring(prefix.Length), out int currentNumber))
            {
                nextNumber = currentNumber + 1;
            }

            return $"{prefix}{nextNumber.ToString("D3")}";
        }

        public async Task CreateRewardAsync(CreateRewardRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RewardName))
                    throw new ArgumentException("RewardName is required");
                var category = await _context.Category
                .Where(c => c.Id == request.CategoryId)
                .FirstOrDefaultAsync();

                var rewardId = Guid.NewGuid();
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                // ✅ Generate RewardCode แบบไม่ซ้ำด้วย prefix จาก Category หรือ fixed prefix เช่น "RD"
                // สมมติ prefix = "RD" (อาจดึงจาก CategoryId ก็ได้)
                var prefix = category.Code;
                var rewardCode = await GenerateUniqueRewardCodeAsync(prefix);

                var reward = new Rewards
                {
                    RewardId = rewardId,
                    RewardName = request.RewardName,
                    PointsRequired = request.PointsRequired,
                    Description = request.Description,
                    CreatedAt = now,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    CouponCode = rewardCode,  // ใช้โค้ดที่ generate ใหม่
                    CategoryId = request.CategoryId,
                    IsActive = request.IsActive
                };

                if (request.Image != null && request.Image.Length > 0)
                {
                    var uploadsPath = _fileSettings.UploadFolder;

                    if (!Directory.Exists(uploadsPath))
                        Directory.CreateDirectory(uploadsPath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.Image.FileName);
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.Image.CopyToAsync(fileStream);
                    }

                    var imageUrl = $"{_fileSettings.BaseUrl.TrimEnd('/')}/{fileName}";
                    reward.ImageUrl = imageUrl.Replace("\\", "/");
                }

                _context.Rewards.Add(reward);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                throw new Exception("Error creating reward: " + innerMessage);
            }
        }


        public async Task<bool> UpdateRewardWithImageAsync(Guid rewardId, UpdateReward updateDto, IFormFile? imageFile)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            var reward = await _context.Rewards.FindAsync(rewardId);
            if (reward == null) return false;

            if (!string.IsNullOrEmpty(updateDto.RewardName))
                reward.RewardName = updateDto.RewardName;

            if (updateDto.PointsRequired.HasValue)
                reward.PointsRequired = updateDto.PointsRequired.Value;

            if (!string.IsNullOrEmpty(updateDto.Description))
                reward.Description = updateDto.Description;

            if (updateDto.StartDate.HasValue && updateDto.StartDate.Value >= new DateTime(1753, 1, 1))
                reward.StartDate = updateDto.StartDate.Value;

            if (updateDto.EndDate.HasValue && updateDto.EndDate.Value >= new DateTime(1753, 1, 1))
                reward.EndDate = updateDto.EndDate.Value;
            if (!string.IsNullOrEmpty(updateDto.CouponCode))
                reward.CouponCode = updateDto.CouponCode;
            if (updateDto.CategoryId.HasValue)
                reward.CategoryId = updateDto.CategoryId.Value;


            reward.UpdateAt = now;

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var savePath = Path.Combine(_fileSettings.UploadFolder, fileName);

                Directory.CreateDirectory(_fileSettings.UploadFolder); // เผื่อโฟลเดอร์ยังไม่ถูกสร้าง

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                reward.ImageUrl = $"{_fileSettings.BaseUrl}/{fileName}".Replace("\\", "/");
            }

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<PagedResult<Rewards>> GetAllRewardsAsync(
    int page,
    int pageSize,
    string? search = null,
    DateTime? startDate = null,
    DateTime? endDate = null,
    bool? isActive = null,
    int? minPoints = null,
    int? maxPoints = null
)
        {
            var query = _context.Rewards.AsQueryable();

            // 🔍 Search by RewardName
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.RewardName.Contains(search));
            }

            // 📅 Filter by date range
            if (startDate.HasValue)
            {
                query = query.Where(r => r.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.EndDate <= endDate.Value);
            }

            // ✅ Filter by IsActive
            if (isActive.HasValue)
            {
                query = query.Where(r => r.IsActive == isActive.Value);
            }

            // 💰 Filter by point range
            if (minPoints.HasValue)
            {
                query = query.Where(r => r.PointsRequired >= minPoints.Value);
            }

            if (maxPoints.HasValue)
            {
                query = query.Where(r => r.PointsRequired <= maxPoints.Value);
            }

            var totalItems = await query.CountAsync();

            var rewards = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Rewards>
            {
                Items = rewards,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }
        public async Task<bool?> ToggleRewardIsActiveAsync(Guid rewardId)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var reward = await _context.Rewards.FindAsync(rewardId);
            if (reward == null)
                return null;

            reward.IsActive = !reward.IsActive;
            reward.UpdateAt = now;

            await _context.SaveChangesAsync();

            return reward.IsActive;
        }


        public async Task<Rewards?> GetRewardByIdAsync(Guid rewardId)
        {
            return await _context.Rewards.FirstOrDefaultAsync(r => r.RewardId == rewardId);
        }
        public async Task<Feeds> CreateFeedAsync(CreateFeedRequest request)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var imageUrls = await SaveImages(request.Images); // List<string> URLs

            var feed = new Feeds
            {
                FeedId = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                CreatedAt = now,
                IsActive = true,
                ImageUrls = imageUrls.Select(url => new ImageUrl { Url = url }).ToList()
            };

            _context.Feeds.Add(feed);
            await _context.SaveChangesAsync();

            return feed;
        }

        public async Task<Feeds?> UpdateFeedAsync(Guid feedId, UpdateFeedRequest request)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var feed = await _context.Feeds
                .Include(f => f.ImageUrls)
                .FirstOrDefaultAsync(f => f.FeedId == feedId);

            if (feed == null) return null;

            // อัปเดตเฉพาะถ้ามีค่า (ไม่บังคับกรอก)
            if (!string.IsNullOrWhiteSpace(request.Title))
                feed.Title = request.Title;

            if (request.Content != null)
                feed.Content = request.Content;

            feed.UpdatedAt = now;

            if (request.Images != null && request.Images.Any())
            {
                var newUrls = await SaveImages(request.Images);
                var newImages = newUrls.Select(url => new ImageUrl { Url = url, FeedId = feed.FeedId }).ToList();
                feed.ImageUrls.AddRange(newImages);
            }

            await _context.SaveChangesAsync();
            return feed;
        }

        public async Task<bool?> ToggleFeedIsActiveAsync(Guid feedId)
        {
            var feed = await _context.Feeds.FindAsync(feedId);
            if (feed == null) return null;

            feed.IsActive = !feed.IsActive;
            feed.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return feed.IsActive;
        }

        public async Task<bool> DeleteImageAsync(int imageId)
        {
            var image = await _context.ImageUrls.FindAsync(imageId);
            if (image == null) return false;

            _context.ImageUrls.Remove(image);
            await _context.SaveChangesAsync();

            // (ถ้าต้องการลบไฟล์จริงในเครื่อง ก็ทำลบไฟล์ในโฟลเดอร์ด้วยที่นี่)

            return true;
        }

        public async Task<PagedResult<FeedDto>> GetFeedsPagedAsync(
     int pageNumber = 1,
     int pageSize = 10,
     string? search = null,
     bool? isActive = null,
     DateTime? startDate = null,
     DateTime? endDate = null)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Feeds.Include(f => f.ImageUrls).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(f => f.Title.Contains(search));
            }

            if (isActive.HasValue)
            {
                query = query.Where(f => f.IsActive == isActive.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(f => f.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(f => f.CreatedAt <= endDate.Value);
            }

            query = query.OrderByDescending(f => f.CreatedAt);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ดึง FeedId ทั้งหมดที่ query ได้
            var feedIds = items.Select(f => f.FeedId).ToList();

            // นับจำนวน Like ที่ IsLike == true สำหรับ feed เหล่านี้
            var likeCounts = await _context.FeedLikes
                .Where(fl => feedIds.Contains(fl.FeedId) && fl.IsLike == true)
                .GroupBy(fl => fl.FeedId)
                .Select(g => new { FeedId = g.Key, Count = g.Count() })
                .ToListAsync();

            // map entity ไป DTO พร้อมเพิ่ม LikeCount
            var dtoItems = items.Select(f =>
            {
                var likeCount = likeCounts.FirstOrDefault(lc => lc.FeedId == f.FeedId)?.Count ?? 0;

                return new FeedDto
                {
                    FeedId = f.FeedId,
                    Title = f.Title,
                    Content = f.Content,
                    ImageUrls = f.ImageUrls,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    IsActive = f.IsActive,
                    LikeCount = likeCount
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


        public async Task<Feeds?> GetFeedByIdAsync(Guid id)
        {
            return await _context.Feeds
        .Include(f => f.ImageUrls)     // โหลด ImageUrls ด้วย
        .FirstOrDefaultAsync(f => f.FeedId == id);
        }
        public async Task<PagedResult<AllTransaction>> GetAllTransactionsAsync(
          int page,
          int pageSize,
          string? search,
          string? transactionType,
          string? rewardName,
          string? phoneNumber,
          DateTime? startDate,
          DateTime? endDate
      )
        {
            var query = _context.PointTransactions.AsQueryable();

            // Join User for phoneNumber
            query = query.Include(t => t.User);

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    t.Description.Contains(search) ||
                    t.RewardName.Contains(search));
            }

            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(t => t.TransactionType == transactionType);
            }

            if (!string.IsNullOrEmpty(rewardName))
            {
                query = query.Where(t => t.RewardName.Contains(rewardName));
            }

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                query = query.Where(t => t.User.PhoneNumber.Contains(phoneNumber));
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= endDate.Value);
            }

            // Order by newest first
            query = query.OrderByDescending(t => t.TransactionDate);

            // Total count before pagination
            var totalCount = await query.CountAsync();

            // Pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new AllTransaction
                {
                    TransactionId = t.TransactionId,
                    UserId = t.UserId,
                    RewardId = t.RewardId,
                    RewardName = t.RewardName,
                    Points = t.Points,
                    TransactionType = t.TransactionType,
                    TransactionDate = t.TransactionDate,
                    Description = t.Description,
                    PhoneNumber = t.User.PhoneNumber
                })
                .ToListAsync();

            return new PagedResult<AllTransaction>
            {
                Items = items,
                TotalItems = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            // จำนวนสมาชิกทั้งหมด, active, inactive
            var totalMembers = await _context.Users.CountAsync();

            var cutoffDate = now.AddDays(-30);

            var activeMembers = await _context.Users
                .Where(u => u.LastTransactionDate.HasValue && u.LastTransactionDate.Value >= cutoffDate)
                .CountAsync();

            var inactiveMembers = totalMembers - activeMembers;

            // จำนวน reward ที่ถูกแลก
            var totalRedeemedRewards = await _context.RedeemedRewards.CountAsync();

            // top 3 reward ที่ถูกแลก พร้อมชื่อรางวัล (join กับ Reward table)
            var top3Rewards = await _context.RedeemedRewards
                .GroupBy(r => r.RewardId)
                .Select(g => new
                {
                    RewardId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(3)
                .Join(_context.Rewards,
                      rr => rr.RewardId,
                      r => r.RewardId,
                      (rr, r) => new RewardCountDto
                      {
                          RewardId = rr.RewardId,
                          RewardName = r.RewardName,
                          Count = rr.Count
                      })
                .ToListAsync();

            // แต้ม earn และ redeem รวม
            var totalEarnPoints = await _context.PointTransactions
                .Where(t => t.TransactionType == "Earn")
                .SumAsync(t => (int?)t.Points) ?? 0;

            var totalRedeemPoints = await _context.PointTransactions
                .Where(t => t.TransactionType == "Redeem")
                .SumAsync(t => (int?)t.Points) ?? 0;

            // **เพิ่ม - จำนวน like ทั้งหมดในระบบ (นับเฉพาะที่ IsLike == true)**
            var totalLikes = await _context.FeedLikes
                .Where(fl => fl.IsLike == true)
                .CountAsync();

            // **เพิ่ม - top 3 feed ที่ถูกกด like เยอะที่สุด**
            var top3Feeds = await _context.FeedLikes
                .Where(fl => fl.IsLike == true)
                .GroupBy(fl => fl.FeedId)
                .Select(g => new
                {
                    FeedId = g.Key,
                    LikeCount = g.Count()
                })
                .OrderByDescending(x => x.LikeCount)
                .Take(3)
                .Join(_context.Feeds,
                      fl => fl.FeedId,
                      f => f.FeedId,
                      (fl, f) => new FeedLikeCountDto
                      {
                          FeedId = fl.FeedId,
                          Title = f.Title,
                          LikeCount = fl.LikeCount
                      })
                .ToListAsync();
            var maleCount = await _context.Users.CountAsync(u => u.Gender == "Male");
            var femaleCount = await _context.Users.CountAsync(u => u.Gender == "Female");
            var otherCount = await _context.Users.CountAsync(u => u.Gender != "Male" && u.Gender != "Female");
            return new DashboardSummaryDto
            {
                Members = new MemberSummaryDto
                {
                    Total = totalMembers,
                    Active = activeMembers,
                    Inactive = inactiveMembers,
                },
                GenderSummary = new GenderSummaryDto
                {
                    Male = maleCount,
                    Female = femaleCount,
                    Other = otherCount
                },
                Rewards = new RewardSummaryDto
                {
                    TotalRedeemed = totalRedeemedRewards,
                    Top3Redeemed = top3Rewards
                },
                Points = new PointSummaryDto
                {
                    Earned = totalEarnPoints,
                    Redeemed = totalRedeemPoints
                },
                Likes = new LikeSummaryDto
                {
                    TotalLikes = totalLikes,
                    Top3LikedFeeds = top3Feeds
                }
            };
        }
        public async Task<Category> CreateCategoryAsync(CreateCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Category name is required");

            var category = new Category
            {
                Name = request.Name,
                Name_En = request.Name_En,
                Code = request.Code,
            };

            _context.Category.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }

    }





}

