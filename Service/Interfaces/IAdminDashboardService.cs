using backend.Models;

namespace backend.Service.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();

    }
}
