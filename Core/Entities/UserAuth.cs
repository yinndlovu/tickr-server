using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class UserAuth
    {
        public int Id { get; set; }
        public int? UserId { get; set; }

        // e.g., "local", "google", "apple"
        public string Provider { get; set; } = null!;
        public string? ProviderUserId { get; set; }

        [Required, MaxLength(75)]
        public string Email { get; set; } = null!;


        [Required, MaxLength(75)]
        public string NormalizedEmail { get; set; } = null!;

        public string? PasswordHash { get; set; } // null for external providers

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
