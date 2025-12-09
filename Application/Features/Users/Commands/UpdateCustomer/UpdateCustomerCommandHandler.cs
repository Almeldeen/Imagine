using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Users.DTOs;
using Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.UpdateCustomer
{
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, BaseResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IImageService _imageService;
        private readonly ILogger<UpdateCustomerCommandHandler> _logger;

        public UpdateCustomerCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IImageService imageService,
            ILogger<UpdateCustomerCommandHandler> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task<BaseResponse<bool>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                return BaseResponse<bool>.FailureResponse("Customer id is required.");
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (user == null)
            {
                return BaseResponse<bool>.FailureResponse("Customer not found.");
            }

            var dto = request.Request ?? new UpdateCustomerDto();

            // Email uniqueness
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existingByEmail = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != user.Id, cancellationToken);

                if (existingByEmail != null)
                {
                    return BaseResponse<bool>.FailureResponse("Email is already in use by another account.");
                }
            }

            // Phone uniqueness
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                var existingByPhone = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber && u.Id != user.Id, cancellationToken);

                if (existingByPhone != null)
                {
                    return BaseResponse<bool>.FailureResponse("Phone number is already in use by another account.");
                }
            }

            // Split full name into first/last
            var fullName = dto.FullName?.Trim() ?? string.Empty;
            var parts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = parts.Length > 0 ? parts[0] : fullName;
            var lastName = parts.Length > 1 ? parts[1] : string.Empty;

            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.IsActive = dto.IsActive;

            // Handle roles
            var targetRoles = (dto.Roles ?? Array.Empty<string>())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!targetRoles.Any())
            {
                // Ensure customer always has at least Client role
                targetRoles.Add("Client");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToRemove = currentRoles
                .Where(r => !targetRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    var msg = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to remove roles from user {UserId}: {Errors}", user.Id, msg);
                    return BaseResponse<bool>.FailureResponse("Failed to update customer roles.");
                }
            }

            var rolesToAdd = targetRoles
                .Where(r => !currentRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (rolesToAdd.Any())
            {
                foreach (var role in rolesToAdd)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        _logger.LogWarning("Attempted to assign non-existing role {Role} to user {UserId}", role, user.Id);
                        continue;
                    }
                }

                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    var msg = string.Join("; ", addResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to add roles to user {UserId}: {Errors}", user.Id, msg);
                    return BaseResponse<bool>.FailureResponse("Failed to update customer roles.");
                }
            }

            // Handle profile image update
            try
            {
                if (request.ProfileImageStream != null && !string.IsNullOrWhiteSpace(request.ProfileImageFileName))
                {
                    var replace = await _imageService.ReplaceImageAsync(
                        request.ProfileImageStream,
                        request.ProfileImageFileName,
                        user.ProfileImageUrl,
                        folder: "avatars",
                        cancellationToken: cancellationToken);

                    if (!replace.Success || string.IsNullOrWhiteSpace(replace.Data))
                    {
                        _logger.LogWarning("Failed to update profile image for user {UserId}: {Message}", user.Id, replace.Message);
                        return BaseResponse<bool>.FailureResponse(replace.Message ?? "Failed to update profile image.");
                    }

                    user.ProfileImageUrl = replace.Data!;
                }
            }
            finally
            {
                request.ProfileImageStream?.Dispose();
            }

            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var msg = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to update customer {UserId}: {Errors}", user.Id, msg);
                return BaseResponse<bool>.FailureResponse("Failed to update customer.");
            }

            return BaseResponse<bool>.SuccessResponse(true, "Customer updated successfully.");
        }
    }
}
