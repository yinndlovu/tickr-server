namespace Core.Entities
{
    // A tracker owned by a user. The Id is generated on the device (the app is
    // offline-first), so the key is (UserId, Id) rather than a server-generated id.
    public class Habit
    {
        public int UserId { get; set; }
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public string? Notes { get; set; }
        public string? Icon { get; set; }

        // Last-write-wins clock, set by the device that made the change.
        public DateTime UpdatedAt { get; set; }

        // Tombstone. Rows are never hard-deleted so a deletion can reach other devices.
        public DateTime? DeletedAt { get; set; }
    }
}
