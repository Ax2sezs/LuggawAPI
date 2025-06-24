using System.Threading.Tasks;
using backend.Models;

namespace backend.Service.Interfaces;

public interface ILineLoginService
{
    Task<LineToken> ExchangeCodeForTokenAsync(string code);
    Task<LineProfile> GetUserProfileAsync(string accessToken);
    string GenerateJwtToken(Guid userId, string displayName);
    Task<UserDetails?> GetUserByIdAsync(string userId);
    Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task LogAsync(Guid? userId, string action, string? oldData = null, string? newData = null);




}
