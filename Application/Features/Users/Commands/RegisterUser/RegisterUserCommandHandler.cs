using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Users.DTOs;
using Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, BaseResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IImageService _imageService;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IImageService imageService,
            ILogger<RegisterUserCommandHandler> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task<BaseResponse<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Request;

            var existingByEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingByEmail != null)
            {
                return BaseResponse<string>.FailureResponse("Email is already registered.");
            }

            var existingByPhone = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber, cancellationToken);
            if (existingByPhone != null)
            {
                return BaseResponse<string>.FailureResponse("Phone number is already registered.");
            }

            var names = (dto.FullName ?? string.Empty).Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = names.Length > 0 ? names[0] : dto.FullName ?? string.Empty;
            var lastName = names.Length > 1 ? names[1] : string.Empty;

            const string defaultProfileImageUrl = "/assets/images/hero-banner.png";
            string profileImageUrl = defaultProfileImageUrl;

            // If a file stream was provided, upload it and use the generated URL
            if (request.ProfileImageStream != null && !string.IsNullOrWhiteSpace(request.ProfileImageFileName))
            {
                var upload = await _imageService.UploadImageAsync(
                    request.ProfileImageStream,
                    request.ProfileImageFileName,
                    folder: "avatars",
                    cancellationToken: cancellationToken);

                request.ProfileImageStream.Dispose();

                if (!upload.Success || string.IsNullOrWhiteSpace(upload.Data))
                {
                    _logger.LogWarning("Failed to upload profile image for {Email}: {Message}", dto.Email, upload.Message);
                    return BaseResponse<string>.FailureResponse(upload.Message ?? "Failed to upload profile image.");
                }

                profileImageUrl = upload.Data!;
            }
            else if (!string.IsNullOrWhiteSpace(dto.ProfileImageUrl))
            {
                profileImageUrl = dto.ProfileImageUrl!;
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                FirstName = firstName,
                LastName = lastName,
                ProfileImageUrl = profileImageUrl,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var message = string.Join("; ", createResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to register user {Email}: {Errors}", dto.Email, message);
                return BaseResponse<string>.FailureResponse(message);
            }

            const string defaultRole = "Client";
            if (await _roleManager.RoleExistsAsync(defaultRole))
            {
                await _userManager.AddToRoleAsync(user, defaultRole);
            }

            _logger.LogInformation("New client user registered with email {Email}", dto.Email);

            return BaseResponse<string>.SuccessResponse(user.Id, "Account created successfully.");
        }
    }
}
