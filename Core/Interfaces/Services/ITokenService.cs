using System.Security.Claims;

namespace Core.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateAuthToken(int userId, string email, string username);
        string GeneratePasswordResetToken(int userId);
        string GenerateEmailVerificationToken(int userId, string email);
        ClaimsPrincipal? ValidateToken(string token);
        string? GetClaim(ClaimsPrincipal principal, string claimType);
    }
}
