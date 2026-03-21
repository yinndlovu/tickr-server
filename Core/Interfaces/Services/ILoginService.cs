using Core.DTOs.Data;
using Core.DTOs.Requests;
using Core.DTOs.Responses;

namespace Core.Interfaces.Services
{
    public interface ILoginService
    {
        Task<AuthResponse<UserDto>> LoginWithEmailOrUsernameAsync(LoginRequest request);
    }
}
