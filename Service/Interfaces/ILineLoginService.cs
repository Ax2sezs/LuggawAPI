using System.Threading.Tasks;
using backend.Models;

namespace backend.Service.Interfaces;

public interface ILineLoginService
{
    Task<LineToken> ExchangeCodeForTokenAsync(string code);
    Task<LineProfile> GetUserProfileAsync(string accessToken);
    string GenerateJwtToken(Guid userId, string displayName, string? role);
    Task<UserDetails?> GetUserByIdAsync(string userId);
    Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task LogAsync(Guid? userId, string action, string? oldData = null, string? newData = null);
    Task<bool> IsPhoneNumberInUseAsync(string phoneNumber, Guid excludeUserId);
    Task<(bool success, string? signature, long timestamp)> UpdatePhoneNumberWithSignatureAsync(Guid userId, string newPhoneNumber);
    Task<bool> IsPhoneNumberAlreadyUsedAsync(string phoneNumber, Guid? currentUserId = null);
    Task<EditProfileRequest> EditProfileNameAsync(Guid userId, EditProfileDto dto);






}
