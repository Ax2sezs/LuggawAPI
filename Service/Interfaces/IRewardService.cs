using backend.Models;

namespace backend.Service.Interfaces
{
    public interface IRewardService
    {
        
        Task<IEnumerable<Rewards>> GetAvailableRewardsAsync(Guid userId); // <-- เพิ่มบรรทัดนี้


    }
}
