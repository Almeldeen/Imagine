using Application.Common.Models;

namespace Application.Common.Interfaces
{
    public interface IImageService
    {
        Task<BaseResponse<string>> UploadImageAsync(Stream imageStream, string fileName, string folder, CancellationToken cancellationToken = default);
        Task<BaseResponse<bool>> DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default);
        Task<BaseResponse<string>> ReplaceImageAsync(Stream newImageStream, string newFileName, string? oldImageUrl, string folder, CancellationToken cancellationToken = default);
        string GetImagePath(string imageUrl);
        bool ImageExists(string imageUrl);
    }
}
