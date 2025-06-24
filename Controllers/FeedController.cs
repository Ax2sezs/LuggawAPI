using System.Security.Claims;
using backend.Models;
using backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;  // เพิ่มบรรทัดนี้


namespace backend.Controllers
{
   
        [ApiController]
        [Route("api/feeds")]
        public class FeedController : ControllerBase
        {
            private readonly IFeedService _feedService;

            public FeedController(IFeedService feedService)
            {
                _feedService = feedService;
            }


        [Authorize]
        [HttpGet("GetUserFeed")]
        public async Task<IActionResult> GetActiveFeeds([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid User ID in token");

            var result = await _feedService.GetActiveFeedsAsync(userId, page, pageSize);
            return Ok(result);
        }



        [HttpPatch("toggle")]
        [Authorize]
        public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeRequest request)
        {
            if (request.FeedId == Guid.Empty)
            {
                return BadRequest("FeedId is required.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _feedService.ToggleLikeAsync(request.FeedId, userId);
            return Ok(new { message = result });
        }

    }

}
