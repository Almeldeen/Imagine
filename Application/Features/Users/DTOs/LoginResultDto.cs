using System.Collections.Generic;

namespace Application.Features.Users.DTOs
{
    public class LoginResultDto
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string FullName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public string? ProfileImageUrl { get; set; }
    }
}
