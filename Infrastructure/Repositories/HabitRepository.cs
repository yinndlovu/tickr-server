using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class HabitRepository(AppDbContext dbContext) : IHabitRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public Task<List<Habit>> GetHabitsAsync(int userId)
        {
            return _dbContext.Habits
                .Where(h => h.UserId == userId)
                .ToListAsync();
        }

        public Task<HabitSetting?> GetSettingAsync(int userId)
        {
            return _dbContext.HabitSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task AddAsync(Habit habit)
        {
            await _dbContext.Habits.AddAsync(habit);
        }

        public async Task AddSettingAsync(HabitSetting setting)
        {
            await _dbContext.HabitSettings.AddAsync(setting);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
