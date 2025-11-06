namespace BackendEcommerce.Domain.Contracts.Services
{
    public interface IMediaUploadService
    {
        // We need a function to upload
        Task<MediaUploadResult> UploadImageAsync(IFormFile file);

        // And a function to "rollback" (delete) if the DB save fails
        Task DeleteImageAsync(string publicId);
    }
    public class MediaUploadResult
    {
        public string PublicId { get; set; } = null!;
        public string Url { get; set; } = null!;
    }
}
