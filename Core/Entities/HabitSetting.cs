namespace Core.Entities
{
    // One row per user: which habit is shown as the main one.
    public class HabitSetting
    {
        public int UserId { get; set; }
        public string? MainHabitId { get; set; }
        public DateTime? MainHabitUpdatedAt { get; set; }
    }
}
