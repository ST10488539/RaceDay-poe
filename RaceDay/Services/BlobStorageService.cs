using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Services
{
    public interface IBlobStorageService
    {
        /// <summary>Returns an error message if the file is not an acceptable image, otherwise null.</summary>
        string? Validate(IFormFile? file);

        /// <summary>Uploads the image to Azure Blob Storage and returns its public URL.</summary>
        Task<string> UploadImageAsync(IFormFile file, string folder);
    }

    public class BlobStorageService : IBlobStorageService
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
        private const long MaxBytes = 5 * 1024 * 1024;

        private readonly IConfiguration _config;

        public BlobStorageService(IConfiguration config)
        {
            _config = config;
        }

        public string? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return "Please choose an image file.";

            if (file.Length > MaxBytes)
                return "The image must be 5 MB or smaller.";

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension) || !AllowedContentTypes.Contains(file.ContentType))
                return "Only JPG, PNG or WEBP images are allowed.";

            return null;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            // Read only when uploading, so the unit tests and CI run without any Azure settings
            string? connectionString = _config["AzureStorage:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("AzureStorage:ConnectionString is not configured.");

            string containerName = _config["AzureStorage:ContainerName"] ?? "raceday-images";

            var container = new BlobContainerClient(connectionString, containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var blob = container.GetBlobClient($"{folder}/{Guid.NewGuid():N}{extension}");

            using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blob.Uri.ToString();
        }
    }
}
