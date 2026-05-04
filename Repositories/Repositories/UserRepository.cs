using Microsoft.AspNetCore.Identity;
using Repositories.IRepositories;
using Repositories.Models;

namespace Repositories.Repositories
{
    public class UserRepository(UserManager<User> userManager) : IUserRepository
    {
        private readonly UserManager<User> _userManager = userManager;

        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            await _userManager.UpdateAsync(user);
            return user;
        }
    }
}
