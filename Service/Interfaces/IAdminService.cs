using backend.Models;

namespace backend.Service.Interfaces
{
    public interface IAdminService
    {
        Task<PagedResult<ShowAllUser>> GetAllUsersAsync(
                int page,
                int pageSize,
                string? searchTerm = null,
                bool? isActive = null,
                DateTime? createdAfter = null,
                DateTime? createdBefore = null
            ); Task ToggleUserStatusAsync(ToggleRequest request);
        Task CreateRewardAsync(CreateRewardRequest request);
        Task<PagedResult<Rewards>> GetAllRewardsAsync(
                int page,
                int pageSize,
                string? search = null,
                DateTime? startDate = null,
                DateTime? endDate = null,
                bool? isActive = null,
                int? minPoints = null,
                int? maxPoints = null
            );

        Task<bool> UpdateRewardWithImageAsync(Guid rewardId, UpdateReward updateDto, IFormFile? imageFile);

        // เปลี่ยนสถานะเปิด/ปิดการใช้งานของรางวัล
        Task<bool?> ToggleRewardIsActiveAsync(Guid rewardId);
        Task<Rewards?> GetRewardByIdAsync(Guid rewardId); // ✅ เมธอดที่เพิ่มใหม่

        Task<Feeds> CreateFeedAsync(CreateFeedRequest request);
        Task<Feeds?> UpdateFeedAsync(Guid feedId, UpdateFeedRequest request);
        Task<PagedResult<FeedDto>> GetFeedsPagedAsync(
                    int pageNumber = 1,
                    int pageSize = 10,
                    string? search = null,
                    bool? isActive = null,
                    DateTime? startDate = null,
                    DateTime? endDate = null); Task<Feeds?> GetFeedByIdAsync(Guid feedId);
        Task<bool?> ToggleFeedIsActiveAsync(Guid feedId);
        Task<bool> DeleteImageAsync(int imageId);
        Task<PagedResult<AllTransaction>> GetAllTransactionsAsync(
                int page,
                int pageSize,
                string? search,
                string? transactionType,
                string? rewardName,
                string? phoneNumber,
                DateTime? startDate,
                DateTime? endDate
            );

        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<Category> CreateCategoryAsync(CreateCategoryRequest request);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<string> GenerateUniqueRewardCodeAsync(string prefix);
        Task<AdminLoginResponse> LoginAsync(AdminLoginRequest request);
        Task<UserRedeemResultDto> GetUsersByRewardAsync(Guid rewardId, int page, int pageSize, string? phoneNumber, bool? isUsed = null, string? couponCode = null);
        Task<bool> RevertCouponUsageAsync(string couponCode);



    }

}
