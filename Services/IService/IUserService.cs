using Dtos;
using Microsoft.AspNetCore.Http;
using Repositories.Models;

namespace Services.IService
{
    public interface IUserService
    {
        Task<bool> ChangePasswordAsync(ChangePasswordDto model);
        Task<User> GetUserAuthenticatedAsync();
        Task<string?> LoginAsync(string email, string password);
        Task<(bool Success, string Message)> RegisterAsync(RegisterDto model);
        Task<UserResponseDto> UploadImageProfileAsync(IFormFile formFile);
    }
}
