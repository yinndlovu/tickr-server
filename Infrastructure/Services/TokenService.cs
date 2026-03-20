using Core.Configuration;
using Core.Enums;
using Core.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenService(JwtSettings jwtSettings)
        {
            _key = jwtSettings.Key;
            _issuer = jwtSettings.Issuer;
            _audience = jwtSettings.Audience;
        }

        public string GenerateAuthToken(int userId, string email, string username)
        {
            var claims = new List<Claim>
            {
                new("sub", userId.ToString()),
                new("email", email),
                new("username", username),
                new("token_type", TokenType.Authentication.ToString().ToLower())
            };

            return CreateToken(claims, TimeSpan.FromDays(30));
        }

        public string GeneratePasswordResetToken(int userId)
        {
            var claims = new List<Claim>
            {
                new("sub", userId.ToString()),
                new("token_type", "password_reset")
            };

            return CreateToken(claims, TimeSpan.FromMinutes(10));
        }

        public string GenerateEmailVerificationToken(int userId, string email)
        {
            var claims = new List<Claim>
            {
                new("sub", userId.ToString()),
                new("email", email),
                new("token_type", "email_verification")
            };

            return CreateToken(claims, TimeSpan.FromHours(24));
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public string? GetClaim(ClaimsPrincipal principal, string claimType)
        {
            return principal.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }

        private string CreateToken(IEnumerable<Claim> claims, TimeSpan expiry)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(expiry),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
