using backend.Models;

namespace backend.Service.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(string userId);

    }
}
