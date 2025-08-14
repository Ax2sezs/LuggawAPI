public interface IOtpService
{
    Task<(bool Success, string? RefCode, string? Token, string? ErrorMessage)> SendOtpAsync(Guid userId, string phoneNumber);
    Task<(bool Success, string? ErrorMessage)> ValidateOtpAsync(string refCode, string token, string otpCode);
}
