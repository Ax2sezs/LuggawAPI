using backend.Models;

namespace backend.Service.Interfaces
{
    public interface IPointService
    {
        Task<int> GetTotalPointsAsync(Guid userId);
        Task EarnPointsAsync(string phoneNumber, int points, string? description);
        Task<PointTransactions> GetTransactionsByUserIdAsync(Guid userId, int pageNumber, int pageSize);

    }
}
