using Application.Common.Interfaces;
using Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateToken(ApplicationUser user, IList<string> roles)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "CHANGE_ME_SUPER_SECRET_KEY";
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("phone_number", user.PhoneNumber ?? string.Empty),
                new Claim("fullName", ($"{user.FirstName} {user.LastName}").Trim())
            };

            if (roles != null && roles.Count > 0)
            {
                // Standard role claims
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

                // Custom roles array claim (comma-separated)
                claims.Add(new Claim("roles", string.Join(",", roles)));
            }

            // IMPORTANT SECURITY NOTE:
            // This token does NOT include an explicit expiration (exp) claim and the
            // TokenValidationParameters are configured with ValidateLifetime = false.
            // This means tokens are effectively non-expiring, which is NOT secure for
            // production systems. For real deployments, you should:
            // - Add an expires / notBefore on the token
            // - Enable ValidateLifetime
            // - Consider introducing refresh tokens / rotation.

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("JWT token generated for user {UserId}", user.Id);
            return tokenString;
        }
    }
}
