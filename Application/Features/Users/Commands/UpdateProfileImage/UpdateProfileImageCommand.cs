using Application.Common.Models;
using MediatR;
using System.IO;

namespace Application.Features.Users.Commands.UpdateProfileImage
{
    public class UpdateProfileImageCommand : IRequest<BaseResponse<string>>
    {
        public string UserId { get; set; } = string.Empty;
        public Stream ImageStream { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
    }
}
