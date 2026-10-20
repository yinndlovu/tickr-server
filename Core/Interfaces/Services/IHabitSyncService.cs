using Core.DTOs.Data;

namespace Core.Interfaces.Services
{
    public interface IHabitSyncService
    {
        // Current cloud state for the user.
        Task<SyncStateDto> GetStateAsync(int userId);

        // Last-write-wins merge of the device state into the cloud. Returns the merged state.
        Task<SyncStateDto> MergeAsync(int userId, SyncStateDto incoming);

        // Makes the cloud state exactly the device state ("keep this device's data").
        Task<SyncStateDto> ReplaceAsync(int userId, SyncStateDto incoming);
    }
}
