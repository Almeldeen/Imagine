using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Users.DTOs;
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
    public class ResetCustomerPasswordCommandHandler : IRequestHandler<ResetCustomerPasswordCommand, BaseResponse<PasswordResetResultDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IPasswordResetEmailTemplate _emailTemplate;
        private readonly ILogger<ResetCustomerPasswordCommandHandler> _logger;

        public ResetCustomerPasswordCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IPasswordResetEmailTemplate emailTemplate,
            ILogger<ResetCustomerPasswordCommandHandler> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _emailTemplate = emailTemplate;
            _logger = logger;
        }

        public async Task<BaseResponse<PasswordResetResultDto>> Handle(ResetCustomerPasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BaseResponse<PasswordResetResultDto>.FailureResponse("A valid customer id is required.");
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return BaseResponse<PasswordResetResultDto>.FailureResponse("Customer not found.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return BaseResponse<PasswordResetResultDto>.FailureResponse("Customer does not have an email address.");
            }

            var oldPasswordHash = user.PasswordHash;
            var oldSecurityStamp = user.SecurityStamp;

            var newPassword = GenerateSecurePassword(12);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!resetResult.Succeeded)
            {
                var msg = string.Join("; ", resetResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to reset password for user {UserId}: {Errors}", user.Id, msg);
                return BaseResponse<PasswordResetResultDto>.FailureResponse("Failed to reset password.");
            }

            try
            {
                var fullName = ($"{user.FirstName} {user.LastName}").Trim();
                var subject = "Your password has been reset";
                var html = _emailTemplate.Build(fullName, newPassword);

                await _emailSender.SendAsync(user.Email, subject, html, cancellationToken);
            }
            catch (Exception ex)
            {
                // Do not log or return the password.
                _logger.LogError(ex, "Password reset succeeded but failed to send email for user {UserId}", user.Id);

                // Best-effort rollback so the customer is not locked out without receiving the new password.
                try
                {
                    user.PasswordHash = oldPasswordHash;
                    user.SecurityStamp = oldSecurityStamp;
                    var rollback = await _userManager.UpdateAsync(user);
                    if (!rollback.Succeeded)
                    {
                        var rollbackMsg = string.Join("; ", rollback.Errors.Select(e => e.Description));
                        _logger.LogWarning(
                            "Failed to rollback password after email failure for user {UserId}: {Errors}",
                            user.Id,
                            rollbackMsg);
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogWarning(
                        rollbackEx,
                        "Exception while attempting to rollback password after email failure for user {UserId}",
                        user.Id);
                }

                return BaseResponse<PasswordResetResultDto>.FailureResponse(
                    "Password reset failed because the email could not be sent. Please try again.");
            }

            _logger.LogInformation("Password reset by admin and emailed successfully for user {UserId}", user.Id);

            return BaseResponse<PasswordResetResultDto>.SuccessResponse(
                new PasswordResetResultDto { EmailSent = true },
                "Password reset successfully. The new password has been sent to the customer’s email.");
        }

        private static string GenerateSecurePassword(int length)
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // no I/O
            const string lower = "abcdefghijkmnopqrstuvwxyz"; // no l
            const string digits = "23456789"; // no 0/1
            const string specials = "@$!%*?&";

            if (length < 8)
            {
                length = 8;
            }

            var allChars = upper + lower + digits + specials;

            // Ensure complexity: at least one char from each category.
            var chars = new char[length];
            var pos = 0;

            chars[pos++] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
            chars[pos++] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
            chars[pos++] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            chars[pos++] = specials[RandomNumberGenerator.GetInt32(specials.Length)];

            for (; pos < length; pos++)
            {
                chars[pos] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
            }

            // Shuffle (Fisher–Yates) using cryptographic RNG.
            for (var i = chars.Length - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }
    }
}
