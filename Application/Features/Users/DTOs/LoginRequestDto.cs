namespace Application.Features.Users.DTOs
{
    public class LoginRequestDto
    {
        // Can be email or phone number
        public string Identifier { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
