using System.Security.Claims;
using backend.Models;
using backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // หรือ "api/points" ถ้าอยาก fix route
    public class PointsController : ControllerBase
    {
        private readonly IPointService _pointService;

        public PointsController(IPointService pointService)
        {
            _pointService = pointService;
        }
        [Authorize]
        [HttpGet("points")]
        public async Task<IActionResult> GetTotalPoints()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid User ID in token");

            var totalPoints = await _pointService.GetTotalPointsAsync(userId);
            return Ok(new { userId, totalPoints });
        }

        [Authorize]
        [HttpGet("transactions")]
        public async Task<IActionResult> GetUserTransactions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid User ID in token");

            var result = await _pointService.GetTransactionsByUserIdAsync(userId, pageNumber, pageSize);
            return Ok(result);
        }



        [HttpPost("earn")]
        public async Task<IActionResult> EarnPoints([FromBody] EarnPointRequest request)
        {
            try
            {
                await _pointService.EarnPointsAsync(request.PhoneNumber, request.Points, request.Description);
                return Ok(new { message = "Points earned successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
