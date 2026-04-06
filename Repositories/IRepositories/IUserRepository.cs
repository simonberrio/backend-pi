using Microsoft.AspNetCore.Identity;
using Repositories.Models;

namespace Repositories.IRepositories
{
    public interface IUserRepository
    {
        Task<IdentityResult> CreateUserAsync(User user, string password);
        Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);
        Task<User?> GetByEmailAsync(string email);
    }
}
