using System.Security.Claims;
using backend.Models;
using backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RedeemController : ControllerBase
{
    private readonly IRedeemService _redeemService;

    public RedeemController(IRedeemService redeemService)
    {
        _redeemService = redeemService;
    }
    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] RedeemRequest request)
    {
        // ดึง userId จาก token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        try
        {
            await _redeemService.RedeemRewardAsync(userId, request.RewardId);
            return Ok(new { message = "Redeem successful" });
        }
        catch (Exception ex)
        {
            // อาจจะทำ logging error ที่นี่ได้
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("redeemed-rewards")]
    public async Task<IActionResult> GetMyRedeemed(
        [FromQuery] string status = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page <= 0 || pageSize <= 0)
            return BadRequest("page and pageSize must be greater than 0");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user token");
        }

        var result = await _redeemService.GetMyRedeemedAsync(userId, status, page, pageSize);
        return Ok(result);
    }




}


