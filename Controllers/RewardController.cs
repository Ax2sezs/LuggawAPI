using backend.Models;
using backend.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RewardController : ControllerBase
    {
        private readonly IRewardService _rewardService;

        public RewardController(IRewardService rewardService)
        {
            _rewardService = rewardService;
        }

       

        [HttpGet("available/{userId}")]
        public async Task<ActionResult<IEnumerable<Rewards>>> GetAvailableRewards(Guid userId)
        {
            try
            {
                var rewards = await _rewardService.GetAvailableRewardsAsync(userId);
                return Ok(rewards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
