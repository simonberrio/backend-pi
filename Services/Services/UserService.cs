using Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.IRepositories;
using Repositories.Models;
using Services.IService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Services
{
    public class UserService(IConfiguration config,
        IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager,
        IUserRepository userRepository) : IUserService
    {
        private readonly IConfiguration _config = config;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto model)
        {
            var user = await GetUserAuthenticatedAsync();

            var result = await _userRepository.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword
            );

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            return true;
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User> GetUserAuthenticatedAsync()
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;

            if (userClaims == null || !userClaims.Identity.IsAuthenticated)
                throw new Exception("Usuario no autenticado");

            var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new Exception("Token inválido");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("Usuario no encontrado");

            return user;
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email) ??
                throw new Exception("Credenciales no válidas");
            var valid = await _userManager.CheckPasswordAsync(user, password);

            if (!valid)
                throw new Exception("Credenciales no válidas");

            return GenerateToken(user);
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto model)
        {
            // Validación básica
            if (model.Password != model.ConfirmPassword)
                return (false, "Las contraseñas no coinciden");

            var existingUser = await _userRepository.GetByEmailAsync(model.Email);

            if (existingUser != null)
                return (false, "El usuario ya existe");

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var result = await _userRepository.CreateUserAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            return (true, "Usuario registrado correctamente");
        }
    }
}
