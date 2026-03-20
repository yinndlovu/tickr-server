using Core.DTOs.Data;
using Core.DTOs.Requests;
using Core.DTOs.Responses;
using Core.Entities;
using Core.Exceptions;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Application.Auth.Services
{
    public class RegisterService(IUserRepository userRepository, IUserAuthRepository userAuthRepository, ITokenService tokenService) : IRegisterService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUserAuthRepository _userAuthRepository = userAuthRepository;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<AuthResponse<UserDto>> RegisterLocalAsync(RegisterRequest request)
        {
            string token;

            var normalizedEmail = request.Email?.Trim();
            if (string.IsNullOrEmpty(normalizedEmail))
            {
                throw new InvalidInputException("An email is required");
            }

            var normalizedName = request.Name?.Trim();
            if (string.IsNullOrEmpty(normalizedName))
            {
                throw new InvalidInputException("Name is required");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                throw new InvalidInputException("Password is required");
            }

            normalizedEmail = normalizedEmail.ToLowerInvariant();

            var localAuthExists = await _userRepository.UserEmailExists(normalizedEmail);
            if (localAuthExists)
            {
                throw new AlreadyExistsException("Email already exists");
            }

            var existingAuth = await _userAuthRepository.FindUserAuthByEmail(normalizedEmail);
            if (existingAuth != null && existingAuth.Provider != "local")
            {
                // check if user profile exists
                if (existingAuth.User != null && existingAuth.UserId != null)
                {
                    return new AuthResponse<UserDto>
                    {
                        Success = true,
                        Message = $"An account with this email already exists via {existingAuth.Provider}. You can log in with {existingAuth.Provider} or set a new password.",
                        Details = new UserDto
                        {
                            Id = existingAuth.UserId.Value,
                            Name = existingAuth.User.Name,
                            Email = existingAuth.Email
                        }
                    };
                }
                if (existingAuth.ProviderUserId != null)
                {

                }
                else
                {
                    return new AuthResponse<UserDto>
                    {
                        Success = false,
                        Message = "Invalid credentials",
                        Details = null
                    };
                }
            }

            bool isValidPassword = _passwordValidator.IsValid(request.Password);
            if (!isValidPassword)
            {
                return new AuthResponse<UserDto>
                {
                    Success = false,
                    Message = "Password is invalid"
                };
            }

            var hashedPassword = _passwordHasher.HashPassword(request.Password);

            var user = new User
            {
                Name = normalizedName,
            };

            var userAuth = new UserAuth
            {
                Provider = AuthProvider.Local.ToString().ToLowerInvariant(),
                NormalizedEmail = normalizedEmail,
                Email = normalizedEmail,
                PasswordHash = hashedPassword,
                User = user
            };

            await _userRepository.AddAsync(user);
            await _userAuthRepository.AddAsync(userAuth);

            bool added = await _appRepository.SaveChangesAsync();
            if (!added)
            {
                return new AuthResponse<UserDto>
                {
                    Success = false,
                    Message = "No user has been added"
                };
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Username = user.Username,
                Email = userAuth.NormalizedEmail,
            };

            token = _tokenService.GenerateAuthToken(
                userId: user.Id,
                email: userAuth.NormalizedEmail,
                username: user.Username
            );

            return new AuthResponse<UserDto>
            {
                Success = true,
                Message = "Successfully registered",
                Token = token,
                Details = userDto
            };
        }
    }
}
