using Dtos;
using Microsoft.AspNetCore.Http;

namespace Services.IService
{
    public interface IImageService
    {
        Task<bool> DeleteImageAsync(string publicId);
        Task<ImageResultDto> UploadImageAsync(IFormFile file);
    }
}
