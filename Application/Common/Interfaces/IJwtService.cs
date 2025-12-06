using Core.Entities;
using System.Collections.Generic;

namespace Application.Common.Interfaces
{
    /// <summary>
    /// Central JWT generation service.
    /// NOTE: In this implementation tokens are configured to be effectively non-expiring
    /// to match the current client requirement. This is NOT recommended for production.
    /// Consider adding expiration and refresh tokens for real-world deployments.
    /// </summary>
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
