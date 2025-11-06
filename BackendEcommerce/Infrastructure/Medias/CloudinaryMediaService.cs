using BackendEcommerce.Domain.Contracts.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
namespace BackendEcommerce.Infrastructure.Medias
{
    // This is the "implementation" of the contract
    public class CloudinaryMediaService : IMediaUploadService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryMediaService(IConfiguration configuration)
        {
            // Read keys from .env (which is loaded into IConfiguration)
            var account = new Account(
                configuration["CLOUDINARY_CLOUD_NAME"],
                configuration["CLOUDINARY_API_KEY"],
                configuration["CLOUDINARY_API_SECRET"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<MediaUploadResult> UploadImageAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                // You can add transformations here (e.g., resizing)
                // Transformation = new Transformation().Width(800).Height(800).Crop("limit")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            return new MediaUploadResult
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.SecureUrl.ToString()
            };
        }

        public async Task DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}
