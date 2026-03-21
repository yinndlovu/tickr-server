using Core.DTOs.Data;
using Core.DTOs.Requests;
using Core.DTOs.Responses;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Application.Auth.Services
{
    public class RegisterService(IUserRepository userRepository, IUserAuthRepository userAuthRepository, ITokenService tokenService,
        IPasswordValidator passwordValidator, IPasswordHasher passwordHasher, IAppRepository appRepository,
        IGoogleTokenVerifier googleTokenVerifier) : IRegisterService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUserAuthRepository _userAuthRepository = userAuthRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IPasswordValidator _passwordValidator = passwordValidator;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly IAppRepository _appRepository = appRepository;
        private readonly IGoogleTokenVerifier _googleTokenVerifier = googleTokenVerifier;

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
                            Email = existingAuth.Email,
                            ProfilePictureUrl = existingAuth.User.ProfilePictureUrl
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
                Name = user.Name,
                Email = userAuth.NormalizedEmail,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            token = _tokenService.GenerateAuthToken(
                userId: user.Id,
                email: userAuth.NormalizedEmail
            );

            return new AuthResponse<UserDto>
            {
                Success = true,
                Message = "Successfully registered",
                Token = token,
                Details = userDto
            };
        }

        public async Task<AuthResponse<UserDto>> RegisterOrLoginGoogleAsync(GoogleAuthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                return new AuthResponse<UserDto>
                {
                    Success = false,
                    Message = "Google ID token is required"
                };
            }

            var googleUser = await _googleTokenVerifier.VerifyIdTokenAsync(request.IdToken);
            if (googleUser == null)
            {
                return new AuthResponse<UserDto>
                {
                    Success = false,
                    Message = "Invalid Google token"
                };
            }

            var normalizedEmail = googleUser.Email.Trim().ToLowerInvariant();
            var provider = AuthProvider.Google.ToString().ToLowerInvariant();

            var existingGoogleAuth = await _userAuthRepository.FindByProviderAndProviderId(provider, googleUser.Subject);
            if (existingGoogleAuth?.User != null)
            {
                if (!string.IsNullOrWhiteSpace(googleUser.Name))
                {
                    existingGoogleAuth.User.Name = googleUser.Name.Trim();
                }

                existingGoogleAuth.User.ProfilePictureUrl = string.IsNullOrWhiteSpace(googleUser.Picture)
                    ? null
                    : googleUser.Picture.Trim();
                await _userRepository.UpdateAsync(existingGoogleAuth.User);
                await _appRepository.SaveChangesAsync();

                var existingToken = _tokenService.GenerateAuthToken(existingGoogleAuth.User.Id, normalizedEmail);
                return new AuthResponse<UserDto>
                {
                    Success = true,
                    Message = "Successfully logged in with Google",
                    Token = existingToken,
                    Details = new UserDto
                    {
                        Id = existingGoogleAuth.User.Id,
                        Name = existingGoogleAuth.User.Name,
                        Email = normalizedEmail,
                        ProfilePictureUrl = existingGoogleAuth.User.ProfilePictureUrl
                    }
                };
            }

            var existingAuthByEmail = await _userAuthRepository.FindUserAuthByEmail(normalizedEmail);
            if (existingAuthByEmail != null)
            {
                existingAuthByEmail.Provider = provider;
                existingAuthByEmail.ProviderUserId = googleUser.Subject;
                await _userAuthRepository.UpdateAsync(existingAuthByEmail);
                var linked = await _appRepository.SaveChangesAsync();
                if (!linked)
                {
                    return new AuthResponse<UserDto>
                    {
                        Success = false,
                        Message = "Failed to link Google account"
                    };
                }

                if (existingAuthByEmail.User != null)
                {
                    if (!string.IsNullOrWhiteSpace(googleUser.Name))
                    {
                        existingAuthByEmail.User.Name = googleUser.Name.Trim();
                    }

                    existingAuthByEmail.User.ProfilePictureUrl = string.IsNullOrWhiteSpace(googleUser.Picture)
                        ? null
                        : googleUser.Picture.Trim();
                    await _userRepository.UpdateAsync(existingAuthByEmail.User);
                    await _appRepository.SaveChangesAsync();

                    var linkedToken = _tokenService.GenerateAuthToken(existingAuthByEmail.User.Id, normalizedEmail);
                    return new AuthResponse<UserDto>
                    {
                        Success = true,
                        Message = "Successfully logged in with Google",
                        Token = linkedToken,
                        Details = new UserDto
                        {
                            Id = existingAuthByEmail.User.Id,
                            Name = existingAuthByEmail.User.Name,
                            Email = normalizedEmail,
                            ProfilePictureUrl = existingAuthByEmail.User.ProfilePictureUrl
                        }
                    };
                }
            }

            var userName = string.IsNullOrWhiteSpace(googleUser.Name)
                ? "Google User"
                : googleUser.Name.Trim();

            var user = new User
            {
                Name = userName,
                ProfilePictureUrl = string.IsNullOrWhiteSpace(googleUser.Picture)
                    ? null
                    : googleUser.Picture.Trim()
            };

            var userAuth = new UserAuth
            {
                Provider = provider,
                ProviderUserId = googleUser.Subject,
                Email = normalizedEmail,
                NormalizedEmail = normalizedEmail,
                User = user
            };

            await _userRepository.AddAsync(user);
            await _userAuthRepository.AddAsync(userAuth);
            var saved = await _appRepository.SaveChangesAsync();

            if (!saved)
            {
                return new AuthResponse<UserDto>
                {
                    Success = false,
                    Message = "Failed to sign in with Google"
                };
            }

            var token = _tokenService.GenerateAuthToken(user.Id, normalizedEmail);
            return new AuthResponse<UserDto>
            {
                Success = true,
                Message = "Successfully signed in with Google",
                Token = token,
                Details = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = normalizedEmail,
                    ProfilePictureUrl = user.ProfilePictureUrl
                }
            };
        }
    }
}
