using backend.Models;
using backend.Service.Interfaces;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/pos")]
public class PosController : ControllerBase
{
    private readonly IPosService _posService;

    public PosController(IPosService posService)
    {
        _posService = posService;
    }

    [HttpGet("get-verify/{couponCode}")]
    public async Task<IActionResult> VerifyCouponByRoute(string couponCode)
    {
        var result = await _posService.GetCouponDetailsAsync(couponCode);
        if (result == null)
        {
            return Ok(new
            {
                isSuccess = false,
                message = "Coupon not found or already used or expired",
                data = (object)null
            });
        }

        return Ok(new
        {
            isSuccess = true,
            message = "success",
            data = result
        });
    }


    // ตรวจสอบคูปอง
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyCoupon([FromBody] VerifyCouponRequest request)
    {
        var result = await _posService.GetCouponDetailsAsync(request.CouponCode);
        if (result == null)
        {
            return Ok(new
            {
                isSuccess = false,
                message = "Coupon not found or already used or expired",
                data = (object)null
            });
        }

        return Ok(new
        {
            isSuccess = true,
            message = "success",
            data = result
        });
    }

    // ใช้คูปอง
    [HttpPut("use/{couponCode}")]
    public async Task<IActionResult> UseCoupon([FromRoute] string couponCode)
    {
        var success = await _posService.MarkCouponAsUsedAsync(couponCode);
        if (!success)
        {
            return BadRequest(new
            {
                isSuccess = false,
                message = "Coupon invalid or already used"
            });
        }

        return Ok(new
        {
            isSuccess = true,
            message = "Coupon marked as used"
        });
    }

}
