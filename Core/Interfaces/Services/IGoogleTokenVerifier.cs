using Core.DTOs.Data;

namespace Core.Interfaces.Services
{
    public interface IGoogleTokenVerifier
    {
        Task<GoogleUserInfoDto?> VerifyIdTokenAsync(string idToken);
    }
}
