using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _uploadsFolder;
        private const int MaxImageWidth = 1920;
        private const int MaxImageHeight = 1080;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public ImageService(
            IWebHostEnvironment environment,
            ILogger<ImageService> logger,
            IConfiguration configuration)
        {
            _environment = environment;
            _logger = logger;
            _configuration = configuration;
            // Ensure WebRootPath is available even if wwwroot folder doesn't exist yet
            var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(AppContext.BaseDirectory, "wwwroot")
                : _environment.WebRootPath;

            // Ensure webroot and uploads directories exist
            if (!Directory.Exists(webRoot))
            {
                Directory.CreateDirectory(webRoot);
            }

            _uploadsFolder = Path.Combine(webRoot, "uploads");
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        public async Task<BaseResponse<string>> UploadImageAsync(Stream imageStream, string fileName, string folder, CancellationToken cancellationToken = default)
        {
            try
            {
                var extension = Path.GetExtension(fileName).ToLowerInvariant();

                if (!_allowedExtensions.Contains(extension))
                {
                    return BaseResponse<string>.FailureResponse($"Invalid file extension. Allowed extensions: {string.Join(", ", _allowedExtensions)}");
                }

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var folderPath = Path.Combine(_uploadsFolder, folder);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, uniqueFileName);

                using (var image = await Image.LoadAsync(imageStream, cancellationToken))
                {
                    if (image.Width > MaxImageWidth || image.Height > MaxImageHeight)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(MaxImageWidth, MaxImageHeight),
                            Mode = ResizeMode.Max
                        }));
                    }

                    await image.SaveAsync(filePath, cancellationToken);
                }

                // Build relative URL under /uploads and then prefix with configured base URL (domain) if available
                var relativeUrl = $"/uploads/{folder}/{uniqueFileName}";
                var baseUrl = _configuration["App:BaseUrl"] ?? _configuration["ImageBaseUrl"];

                string imageUrl = string.IsNullOrWhiteSpace(baseUrl)
                    ? relativeUrl
                    : $"{baseUrl.TrimEnd('/')}{relativeUrl}";

                _logger.LogInformation("Image uploaded successfully: {ImageUrl}", imageUrl);

                return BaseResponse<string>.SuccessResponse(imageUrl, "Image uploaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image: {FileName}", fileName);
                return BaseResponse<string>.FailureResponse($"Failed to upload image: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return BaseResponse<bool>.SuccessResponse(true, "No image to delete");
                }

                var filePath = GetImagePath(imageUrl);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Image file not found: {FilePath}", filePath);
                    return BaseResponse<bool>.SuccessResponse(true, "Image file not found");
                }

                await Task.Run(() => File.Delete(filePath), cancellationToken);

                _logger.LogInformation("Image deleted successfully: {ImageUrl}", imageUrl);

                return BaseResponse<bool>.SuccessResponse(true, "Image deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
                return BaseResponse<bool>.FailureResponse($"Failed to delete image: {ex.Message}");
            }
        }

        public async Task<BaseResponse<string>> ReplaceImageAsync(Stream newImageStream, string newFileName, string? oldImageUrl, string folder, CancellationToken cancellationToken = default)
        {
            try
            {
                var uploadResult = await UploadImageAsync(newImageStream, newFileName, folder, cancellationToken);

                if (!uploadResult.Success)
                {
                    return uploadResult;
                }

                if (!string.IsNullOrWhiteSpace(oldImageUrl))
                {
                    await DeleteImageAsync(oldImageUrl, cancellationToken);
                }

                return uploadResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing image");
                return BaseResponse<string>.FailureResponse($"Failed to replace image: {ex.Message}");
            }
        }

        public string GetImagePath(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return string.Empty;
            }

            // Support both absolute URLs (with domain) and relative paths
            string pathPart = imageUrl;
            if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            {
                pathPart = uri.AbsolutePath;
            }

            var relativePath = pathPart.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(AppContext.BaseDirectory, "wwwroot")
                : _environment.WebRootPath;
            return Path.Combine(webRoot, relativePath);
        }

        public bool ImageExists(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return false;
            }

            var filePath = GetImagePath(imageUrl);
            return File.Exists(filePath);
        }
    }
}
