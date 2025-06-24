using backend.Models;

namespace backend.Service.Interfaces
{
    public interface IFeedService
    {
        Task<PagedResult<FeedDto>> GetActiveFeedsAsync(Guid userId, int pageNumber, int pageSize);
        Task<string> ToggleLikeAsync(Guid feedId, Guid userId);


    }
}
