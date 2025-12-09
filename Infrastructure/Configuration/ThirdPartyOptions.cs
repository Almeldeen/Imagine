using System;

namespace Infrastructure.Configuration
{
    public class ThirdPartyOptions
    {
        public string DeApiKey { get; set; } = string.Empty;
        public string TryOnKey { get; set; } = string.Empty;
        public string DeApiBase { get; set; } = "https://api.deapi.ai/api/v1";
        public string TryOnBase { get; set; } = "https://tryon-api.com/api/v1";
    }
}
