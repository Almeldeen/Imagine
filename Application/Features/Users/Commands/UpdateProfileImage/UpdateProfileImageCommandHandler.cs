using Application.Common.Interfaces;
using Application.Common.Models;
using Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.UpdateProfileImage
{
    public class UpdateProfileImageCommandHandler : IRequestHandler<UpdateProfileImageCommand, BaseResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageService _imageService;
        private readonly ILogger<UpdateProfileImageCommandHandler> _logger;

        public UpdateProfileImageCommandHandler(
            UserManager<ApplicationUser> userManager,
            IImageService imageService,
            ILogger<UpdateProfileImageCommandHandler> logger)
        {
            _userManager = userManager;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task<BaseResponse<string>> Handle(UpdateProfileImageCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null || !user.IsActive)
            {
                return BaseResponse<string>.FailureResponse("User not found or inactive.");
            }

            try
            {
                var upload = await _imageService.ReplaceImageAsync(
                    request.ImageStream,
                    request.FileName,
                    user.ProfileImageUrl,
                    folder: "avatars",
                    cancellationToken: cancellationToken);

                if (!upload.Success || string.IsNullOrWhiteSpace(upload.Data))
                {
                    _logger.LogWarning("Failed to update profile image for user {UserId}: {Message}", user.Id, upload.Message);
                    return BaseResponse<string>.FailureResponse(upload.Message ?? "Failed to update profile image.");
                }

                user.ProfileImageUrl = upload.Data!;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var message = string.Join("; ", updateResult.Errors);
                    _logger.LogWarning("Failed to persist new profile image for user {UserId}: {Errors}", user.Id, message);
                    return BaseResponse<string>.FailureResponse("Failed to update profile image.");
                }

                return BaseResponse<string>.SuccessResponse(upload.Data!, "Profile image updated successfully.");
            }
            finally
            {
                request.ImageStream?.Dispose();
            }
        }
    }
}
