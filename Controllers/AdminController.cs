using backend.Models;
using backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/admin/")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
        {
            try
            {
                var response = await _adminService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // เพิ่ม log error ที่นี่
                Console.WriteLine($"Login error: {ex}");
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Viewer")]
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _adminService.GetDashboardSummaryAsync();
            return Ok(result);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("all-user")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] DateTime? createdAfter = null,
            [FromQuery] DateTime? createdBefore = null
        )
        {
            var users = await _adminService.GetAllUsersAsync(
                page,
                pageSize,
                searchTerm,
                isActive,
                createdAfter,
                createdBefore
            );
            return Ok(users);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("get-all-transaction")]
        public async Task<IActionResult> GetAllTransactions(
            int page = 1,
            int pageSize = 20,
            string? search = null,
            string? transactionType = null,
            string? rewardName = null,
            string? phoneNumber = null,
            DateTime? startDate = null,
            DateTime? endDate = null
        )
        {
            var result = await _adminService.GetAllTransactionsAsync(
                page,
                pageSize,
                search,
                transactionType,
                rewardName,
                phoneNumber,
                startDate,
                endDate
            );

            return Ok(result);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("toggle-user-status")]
        public async Task<IActionResult> ToggleUserStatus([FromBody] ToggleRequest request)
        {
            try
            {
                await _adminService.ToggleUserStatusAsync(request);
                return Ok(new { success = true, message = "User status updated." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPatch("toggle-user-policy/{userId}")]
        public async Task<IActionResult> ToggleUserPolicy(Guid userId)
        {
            try
            {
                await _adminService.ToggleUserPolicyAsync(userId);
                return Ok(new { success = true, message = "User Policy updated." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("add-cate")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                var category = await _adminService.CreateCategoryAsync(request);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("get-category")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _adminService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("generate-code")]
        public async Task<IActionResult> GenerateCode([FromQuery] string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                return BadRequest("Prefix is required");

            try
            {
                var code = await _adminService.GenerateUniqueRewardCodeAsync(prefix);
                return Ok(new { code });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateReward([FromForm] CreateRewardRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _adminService.CreateRewardAsync(request);
                return Ok(new { message = "Reward created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("update-reward/{rewardId}")]
        public async Task<IActionResult> UpdateReward(
            Guid rewardId,
            [FromForm] UpdateReward updateDto,
            IFormFile? imageFile
        )
        {
            var result = await _adminService.UpdateRewardWithImageAsync(
                rewardId,
                updateDto,
                imageFile
            );
            if (!result)
                return NotFound();

            return Ok(); // หรือ Ok() ถ้าอยากส่งข้อมูลกลับ
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPatch("rewards/{rewardId}/toggle-active")]
        public async Task<IActionResult> ToggleRewardStatus(Guid rewardId)
        {
            var result = await _adminService.ToggleRewardIsActiveAsync(rewardId);
            if (result == null)
                return NotFound(new { message = "Reward not found" });

            return Ok(new { isActive = result });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("rewards")]
        public async Task<IActionResult> GetAllRewards(
            int page = 1,
            int pageSize = 10,
            string? search = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool? isActive = null,
            int? minPoints = null,
            int? maxPoints = null
        )
        {
            var result = await _adminService.GetAllRewardsAsync(
                page,
                pageSize,
                search,
                startDate,
                endDate,
                isActive,
                minPoints,
                maxPoints
            );
            return Ok(result);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("reward/{rewardId}")]
        public async Task<IActionResult> GetRewardById(Guid rewardId)
        {
            var reward = await _adminService.GetRewardByIdAsync(rewardId);
            if (reward == null)
                return NotFound(new { message = "Reward not found" });

            return Ok(reward);
        }

        [Authorize(Roles = "SuperAdmin,Viewer")]
        [HttpGet("export")]
        public async Task<IActionResult> ExportRedeemedUsers(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate
    )
        {
            if (startDate > endDate)
            {
                return BadRequest("startDate must be less than or equal to endDate");
            }

            var fileBytes = await _adminService.ExportRedeemedUsersAsync(startDate, endDate);

            var fileName = $"redeemed_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }




        [Authorize(Roles = "Admin")]
        [HttpPost("create-feed")]
        public async Task<IActionResult> CreateFeed([FromForm] CreateFeedRequest request)
        {
            var feed = await _adminService.CreateFeedAsync(request);
            return Ok(feed);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update-feed/{id}")]
        public async Task<IActionResult> UpdateFeed(Guid id, [FromForm] UpdateFeedRequest request)
        {
            var updatedFeed = await _adminService.UpdateFeedAsync(id, request);
            if (updatedFeed == null)
                return NotFound();
            return Ok(updatedFeed);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{feedId}/toggle-active")]
        public async Task<IActionResult> ToggleFeedActive(Guid feedId)
        {
            var newStatus = await _adminService.ToggleFeedIsActiveAsync(feedId);
            if (newStatus == null)
                return NotFound(new { message = "Feed not found" });

            return Ok(new { isActive = newStatus });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var deleted = await _adminService.DeleteImageAsync(imageId);
            if (!deleted)
                return NotFound(new { message = "Image not found" });

            return Ok();
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("get-all-feed")]
        public async Task<IActionResult> GetFeeds(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null
        )
        {
            var pagedFeeds = await _adminService.GetFeedsPagedAsync(
                pageNumber,
                pageSize,
                search,
                isActive,
                startDate,
                endDate
            );

            return Ok(pagedFeeds);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("get-feed-by/{id}")]
        public async Task<IActionResult> GetFeedById(Guid id)
        {
            var feed = await _adminService.GetFeedByIdAsync(id);
            if (feed == null)
                return NotFound();
            return Ok(feed);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("reward/{rewardId}/users")]
        public async Task<ActionResult<UserRedeemResultDto>> GetUsersByReward(
            Guid rewardId,
            int page = 1,
            int pageSize = 20,
            string? phoneNumber = null,
            bool? isUsed = null,
            string? couponCode = null
        )
        {
            var result = await _adminService.GetUsersByRewardAsync(
                rewardId,
                page,
                pageSize,
                phoneNumber,
                isUsed,
                couponCode
            );
            return Ok(result);
        }

        [Authorize(Roles = "SuperAdmin,Viewer")]
        [HttpGet("reward/transaction")]
        public async Task<ActionResult<RedeemTransactionResultDto>> GetRewardTransaction(
     int page = 1,
     int pageSize = 20,
     string? phoneNumber = null,
     bool? isUsed = null,
     string? couponCode = null,
     string? rewardName = null,
     DateTime? startDate = null,
     DateTime? endDate = null
 )
        {
            var result = await _adminService.GetRewardTransactionsAsync(
                page,
                pageSize,
                phoneNumber,
                isUsed,
                couponCode,
                rewardName,
                startDate,
                endDate
            );

            return Ok(result);
        }


        [Authorize(Roles = "SuperAdmin")]
        [HttpPatch("coupon/revert/{couponCode}")]
        public async Task<IActionResult> RevertCoupon([FromRoute] string couponCode)
        {
            var success = await _adminService.RevertCouponUsageAsync(couponCode);

            if (!success)
            {
                return BadRequest(
                    new { isSuccess = false, message = "Coupon invalid or not used yet" }
                );
            }

            return Ok(new { isSuccess = true, message = "Coupon reverted successfully" });
        }

        //// PUT api/admin/users/{id}/toggle-status
        //[HttpPut("{id}/toggle-status")]
        //public async Task<IActionResult> ToggleUserStatus(Guid id)
        //{
        //    try
        //    {
        //        await _adminService.ToggleUserStatusAsync(id);
        //        return NoContent();
        //    }
        //    catch (KeyNotFoundException)
        //    {
        //        return NotFound($"User with id {id} not found.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error: {ex.Message}");
        //    }
        //}
    }
}
