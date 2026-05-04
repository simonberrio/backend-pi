using Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.IService;

namespace MyApp.Api.Controllers
{
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            await _userService.ChangePasswordAsync(model);
            return Ok(new { message = "Contraseña cambiada correctamente" });
        }

        [HttpGet("GetUserAuthenticated")]
        [Authorize]
        public async Task<IActionResult> GetUserAuthenticated()
        {
            User userAuthenticated = await _userService.GetUserAuthenticatedAsync();
            return Ok(new { message = $"UserName= {userAuthenticated.UserName}" });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var token = await _userService.LoginAsync(model.Email, model.Password);

            if (token == null)
                return Unauthorized();

            return Ok(new { token });
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var result = await _userService.RegisterAsync(model);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost("UploadImageProfileAsync")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImageProfileAsync(IFormFile file)
        {
            var result = await _userService.UploadImageProfileAsync(file);
            return Ok(result);
        }
    }
}
