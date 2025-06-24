using System;
using System.Threading.Tasks;
using backend.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace backend.Service.Interfaces
{
    public interface IRedeemService
    {
        Task RedeemRewardAsync(Guid userId, Guid rewardId);
        Task<PagedResult<UserRedeemed>> GetMyRedeemedAsync(Guid userId, string status, int page, int pageSize);

    }
}
