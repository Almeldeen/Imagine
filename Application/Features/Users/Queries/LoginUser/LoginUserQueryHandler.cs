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

namespace Application.Features.Users.Queries.LoginUser
{
    public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, BaseResponse<LoginResultDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ILogger<LoginUserQueryHandler> _logger;

        public LoginUserQueryHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            ILogger<LoginUserQueryHandler> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<BaseResponse<LoginResultDto>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
        {
            var dto = request.Request;
            var identifier = dto.Identifier?.Trim() ?? string.Empty;

            ApplicationUser? user = null;

            // Simple heuristic: if identifier looks like an email, try email first
            var looksLikeEmail = identifier.Contains("@");
            if (looksLikeEmail)
            {
                user = await _userManager.FindByEmailAsync(identifier);
            }

            if (user == null)
            {
                user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == identifier, cancellationToken);
            }

            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Failed login attempt for identifier {Identifier}: user not found or inactive", identifier);
                return BaseResponse<LoginResultDto>.FailureResponse("Invalid email/phone or password.");
            }

            // Use SignInManager to respect lockout settings
            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (!signInResult.Succeeded)
            {
                _logger.LogWarning("Failed login attempt for user {UserId}: invalid password", user.Id);
                return BaseResponse<LoginResultDto>.FailureResponse("Invalid email/phone or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user, roles);

            var profileImageUrl = string.IsNullOrWhiteSpace(user.ProfileImageUrl)
                ? "/assets/images/hero-banner.png"
                : user.ProfileImageUrl;

            var result = new LoginResultDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                FullName = ($"{user.FirstName} {user.LastName}").Trim(),
                Roles = roles.ToList(),
                ProfileImageUrl = profileImageUrl
            };

            _logger.LogInformation("User {UserId} logged in successfully with roles: {Roles}", user.Id, string.Join(",", roles));

            return BaseResponse<LoginResultDto>.SuccessResponse(result, "Login successful.");
        }
    }
}
