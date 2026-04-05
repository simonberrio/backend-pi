using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IService
{
    public interface IUserService
    {
        Task<string?> LoginAsync(string email, string password);
        Task<(bool Success, string Message)> RegisterAsync(RegisterDto model);
    }
}
