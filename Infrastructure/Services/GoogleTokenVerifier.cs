using Core.Configuration;
using Core.DTOs.Data;
using Core.Interfaces.Services;
using Google.Apis.Auth;

namespace Infrastructure.Services
{
    public class GoogleTokenVerifier(GoogleAuthSettings settings) : IGoogleTokenVerifier
    {
        private readonly GoogleAuthSettings _settings = settings;

        public async Task<GoogleUserInfoDto?> VerifyIdTokenAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken) || string.IsNullOrWhiteSpace(_settings.ClientId))
            {
                return null;
            }

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = [_settings.ClientId]
                    }
                );

                if (string.IsNullOrWhiteSpace(payload.Subject) || string.IsNullOrWhiteSpace(payload.Email))
                {
                    return null;
                }

                return new GoogleUserInfoDto
                {
                    Subject = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
