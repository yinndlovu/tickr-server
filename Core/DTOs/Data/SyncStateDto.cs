namespace Core.DTOs.Data
{
    public class SyncStateDto
    {
        public List<HabitDto> Habits { get; set; } = [];
        public string? MainHabitId { get; set; }
        public DateTime? MainHabitUpdatedAt { get; set; }
    }
}
