using Core.Entities;

namespace Core.Interfaces.Repositories
{
    public interface IHabitRepository
    {
        Task<List<Habit>> GetHabitsAsync(int userId);
        Task<HabitSetting?> GetSettingAsync(int userId);
        Task AddAsync(Habit habit);
        Task AddSettingAsync(HabitSetting setting);

        // Unlike the other repositories this does not swallow exceptions:
        // a failed sync must surface as an error so the app retries it.
        Task SaveChangesAsync();
    }
}
