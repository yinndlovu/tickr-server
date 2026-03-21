namespace Core.DTOs.Data
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public required string Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
