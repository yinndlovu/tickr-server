namespace Core.DTOs.Requests
{
    public class GoogleAuthRequest
    {
        public required string ProviderUserId { get; init; }
        public required string Email { get; init; }
        public string? DisplayName { get; init; }
        public string? AvatarUrl { get; init; }
    }
}
