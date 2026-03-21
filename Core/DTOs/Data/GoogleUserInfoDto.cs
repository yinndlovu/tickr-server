namespace Core.DTOs.Data
{
    public class GoogleUserInfoDto
    {
        public required string Subject { get; init; }
        public required string Email { get; init; }
        public string? Name { get; init; }
        public string? Picture { get; init; }
    }
}
