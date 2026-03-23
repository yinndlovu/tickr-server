using Core.DTOs.Data;
using Core.DTOs.Requests;
using Core.DTOs.Responses;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Application.Auth.Services
{
    public class LoginService(ITokenService tokenService, IUserAuthRepository userAuthRepository, IPasswordHasher passwordHasher) : ILoginService
    {
        private readonly IUserAuthRepository _userAuthRepository = userAuthRepository;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<AuthResponse<UserDto>> LoginWithEmailAsync(LoginRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var userAuth = await _userAuthRepository.FindUserAuthByEmail(normalizedEmail);
            if (userAuth == null)
            {
                return new AuthResponse<UserDto>
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }

            if (userAuth.User == null)
            {
                return new AuthResponse<UserDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            if (userAuth.PasswordHash == null)
            {
                return new AuthResponse<UserDto>
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }

            if (!_passwordHasher.VerifyPassword(request.Password, userAuth.PasswordHash))
            {
                return new AuthResponse<UserDto>
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }

            var token = _tokenService.GenerateAuthToken(
                userId: userAuth.User.Id,
                email: userAuth.NormalizedEmail
            );

            return new AuthResponse<UserDto>
            {
                Success = true,
                Message = "Successfully logged in",
                Token = token,
                Details = new UserDto
                {
                    Id = userAuth.User.Id,
                    Name = userAuth.User.Name,
                    Email = userAuth.NormalizedEmail,
                    ProfilePictureUrl = userAuth.User.ProfilePictureUrl
                }
            };
        }
    }
}
