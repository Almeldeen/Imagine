using Application.Common.Models;
using Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.ResetCustomerPassword
{
    public class ResetCustomerPasswordCommandHandler : IRequestHandler<ResetCustomerPasswordCommand, BaseResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ResetCustomerPasswordCommandHandler> _logger;

        public ResetCustomerPasswordCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILogger<ResetCustomerPasswordCommandHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<BaseResponse<string>> Handle(ResetCustomerPasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BaseResponse<string>.FailureResponse("A valid customer id is required.");
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return BaseResponse<string>.FailureResponse("Customer not found.");
            }

            var newPassword = GenerateSecurePassword(12);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!resetResult.Succeeded)
            {
                var msg = string.Join("; ", resetResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to reset password for user {UserId}: {Errors}", user.Id, msg);
                return BaseResponse<string>.FailureResponse("Failed to reset password.");
            }

            _logger.LogInformation("Password reset by admin for user {UserId}", user.Id);

            return BaseResponse<string>.SuccessResponse(newPassword, "Password reset successfully.");
        }

        private static string GenerateSecurePassword(int length)
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // no I/O
            const string lower = "abcdefghijkmnopqrstuvwxyz"; // no l
            const string digits = "23456789"; // no 0/1
            const string specials = "@$!%*?&";

            var allChars = upper + lower + digits + specials;
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                var idx = bytes[i] % allChars.Length;
                sb.Append(allChars[idx]);
            }

            return sb.ToString();
        }
    }
}
