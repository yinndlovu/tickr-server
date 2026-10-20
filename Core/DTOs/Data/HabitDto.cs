namespace Core.DTOs.Data
{
    public class HabitDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required DateTime StartDate { get; set; }
        public string? Notes { get; set; }
        public string? Icon { get; set; }
        public required DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
