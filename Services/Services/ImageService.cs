using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Services.IService;

namespace Services.Services
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;
        public ImageService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                throw new Exception("PublicId no válido");

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);

            return result.Result == "ok";
        }

        public async Task<ImageResultDto> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Archivo no válido");

            if (!file.ContentType.StartsWith("image/"))
                throw new Exception("Archivo no válido");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "myapp"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception(result.Error.Message);

            return new ImageResultDto
            {
                Url = result.SecureUrl.ToString(),
                PublicId = result.PublicId
            };
        }
    }
}
