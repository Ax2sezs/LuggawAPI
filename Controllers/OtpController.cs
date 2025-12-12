using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/otp")]
public class OtpController : ControllerBase
{
    private readonly IOtpService _otpService;

    public OtpController(IOtpService otpService)
    {
        _otpService = otpService;
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        var userId = Guid.NewGuid(); // หรือใช้จาก Auth
        var result = await _otpService.SendOtpAsync(userId, request.PhoneNumber);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(new
        {
            RefCode = result.RefCode,
            Token = result.Token
        });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await _otpService.ValidateOtpAsync(request.RefCode, request.Token, request.Otp);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(new { success = true, message = "OTP ยืนยันสำเร็จ" });
    }
}

