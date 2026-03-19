using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public DateTime CreatedAt = DateTime.UtcNow;
        public DateTime UpdatedAt = DateTime.UtcNow;
        public ICollection<UserAuth> AuthMethods { get; set; } = [];
        public ICollection<UserSession> Sessions { get; set; } = [];
    }
}
