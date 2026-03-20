using Core.DTOs.Data;
using Core.DTOs.Requests;
using Core.DTOs.Responses;

namespace Core.Interfaces.Services
{
    public interface IRegisterService
    {
        Task<AuthResponse<UserDto>> RegisterLocalAsync(RegisterRequest request);
    }
}
