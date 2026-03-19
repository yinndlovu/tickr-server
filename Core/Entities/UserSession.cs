using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class UserSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RefreshTokenHash { get; set; } = null!;
        public string? Device { get; set; }
        public string? IP { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        public User User { get; set; } = null!;
    }
}
