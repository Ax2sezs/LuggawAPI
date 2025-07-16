using backend.Models;

namespace backend.Service.Interfaces
{
    public interface IPointService
    {
        Task<TotalPoint> GetTotalPointsAsync(Guid userId);
        Task EarnPointsAsync(string phoneNumber, int points, string? description);
        Task<PointTransactions> GetTransactionsByPhoneNumberAsync(string phoneNumber, int pageNumber, int pageSize);
        Task<string?> GetUserPhoneNumberAsync(Guid userId);


    }
}
