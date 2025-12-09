using Application.Common.Models;
using Application.Features.Users.DTOs;
using Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.ImportCustomers
{
    public class ImportCustomersCommandHandler : IRequestHandler<ImportCustomersCommand, BaseResponse<ImportCustomersResultDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ImportCustomersCommandHandler> _logger;

        public ImportCustomersCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILogger<ImportCustomersCommandHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<BaseResponse<ImportCustomersResultDto>> Handle(ImportCustomersCommand request, CancellationToken cancellationToken)
        {
            if (request.FileStream == null || request.FileStream == Stream.Null)
            {
                return BaseResponse<ImportCustomersResultDto>.FailureResponse("Import file is required.");
            }

            var result = new ImportCustomersResultDto();

            try
            {
                using var reader = new StreamReader(request.FileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);

                // Read header
                var headerLine = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(headerLine))
                {
                    return BaseResponse<ImportCustomersResultDto>.FailureResponse("Import file is empty.");
                }

                var headerParts = headerLine.Split(',');
                int idxFullName = Array.FindIndex(headerParts, h => string.Equals(h.Trim(), "FullName", StringComparison.OrdinalIgnoreCase));
                int idxEmail = Array.FindIndex(headerParts, h => string.Equals(h.Trim(), "Email", StringComparison.OrdinalIgnoreCase));
                int idxPhone = Array.FindIndex(headerParts, h => string.Equals(h.Trim(), "PhoneNumber", StringComparison.OrdinalIgnoreCase));

                if (idxEmail < 0)
                {
                    return BaseResponse<ImportCustomersResultDto>.FailureResponse("Import file must contain an Email column.");
                }

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var parts = line.Split(',');

                    string GetPart(int index)
                    {
                        if (index < 0 || index >= parts.Length) return string.Empty;
                        return parts[index].Trim().Trim('"');
                    }

                    var email = GetPart(idxEmail);
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        result.Skipped++;
                        continue;
                    }

                    var existing = await _userManager.FindByEmailAsync(email);
                    if (existing != null)
                    {
                        // Skip existing users to avoid duplicates
                        result.Skipped++;
                        continue;
                    }

                    var fullName = idxFullName >= 0 ? GetPart(idxFullName) : string.Empty;
                    var phone = idxPhone >= 0 ? GetPart(idxPhone) : string.Empty;

                    var (firstName, lastName) = SplitFullName(fullName);

                    var user = new ApplicationUser
                    {
                        Email = email,
                        UserName = email,
                        FirstName = firstName,
                        LastName = lastName,
                        PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone,
                        EmailConfirmed = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };

                    var password = GenerateSecurePassword(12);

                    var createResult = await _userManager.CreateAsync(user, password);
                    if (!createResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to create imported customer {Email}: {Errors}", email, string.Join("; ", createResult.Errors.Select(e => e.Description)));
                        result.Skipped++;
                        continue;
                    }

                    // Always assign Client role
                    await _userManager.AddToRoleAsync(user, "Client");

                    result.Imported++;
                }

                return BaseResponse<ImportCustomersResultDto>.SuccessResponse(result, "Customers imported successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while importing customers from file {FileName}", request.FileName ?? "(unknown)");
                return BaseResponse<ImportCustomersResultDto>.FailureResponse("An error occurred while importing customers.");
            }
        }

        private static (string FirstName, string LastName) SplitFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return ("Customer", string.Empty);
            }

            var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return (parts[0], string.Empty);
            }

            return (parts[0], parts[1]);
        }

        private static string GenerateSecurePassword(int length)
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string specials = "@$!%*?&";

            var allChars = upper + lower + digits + specials;
            var rnd = new Random();
            var chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                var idx = rnd.Next(allChars.Length);
                chars[i] = allChars[idx];
            }

            return new string(chars);
        }
    }
}
