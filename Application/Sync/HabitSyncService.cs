using Core.DTOs.Data;
using Core.Entities;
using Core.Exceptions;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Application.Sync
{
    public class HabitSyncService(IHabitRepository habitRepository) : IHabitSyncService
    {
        private const int MaxHabits = 1000;
        private const int MaxIdLength = 64;

        private readonly IHabitRepository _habitRepository = habitRepository;

        public async Task<SyncStateDto> GetStateAsync(int userId)
        {
            var habits = await _habitRepository.GetHabitsAsync(userId);
            var setting = await _habitRepository.GetSettingAsync(userId);
            return ToState(habits, setting);
        }

        public async Task<SyncStateDto> MergeAsync(int userId, SyncStateDto incoming)
        {
            Validate(incoming);

            var habits = await _habitRepository.GetHabitsAsync(userId);
            var byId = habits.ToDictionary(h => h.Id);

            foreach (var dto in incoming.Habits)
            {
                var incomingUpdatedAt = ToUtc(dto.UpdatedAt);

                if (byId.TryGetValue(dto.Id, out var existing))
                {
                    // newer write wins; on a tie the cloud copy stays
                    if (incomingUpdatedAt > existing.UpdatedAt)
                    {
                        Apply(existing, dto);
                    }
                    continue;
                }

                var habit = new Habit { UserId = userId, Id = dto.Id };
                Apply(habit, dto);
                await _habitRepository.AddAsync(habit);
                byId[habit.Id] = habit;
            }

            var setting = await _habitRepository.GetSettingAsync(userId);
            if (incoming.MainHabitUpdatedAt is DateTime incomingMainAt)
            {
                var mainAt = ToUtc(incomingMainAt);

                if (setting == null)
                {
                    setting = new HabitSetting
                    {
                        UserId = userId,
                        MainHabitId = incoming.MainHabitId,
                        MainHabitUpdatedAt = mainAt
                    };
                    await _habitRepository.AddSettingAsync(setting);
                }
                else if (setting.MainHabitUpdatedAt == null || mainAt > setting.MainHabitUpdatedAt)
                {
                    setting.MainHabitId = incoming.MainHabitId;
                    setting.MainHabitUpdatedAt = mainAt;
                }
            }

            await _habitRepository.SaveChangesAsync();
            return ToState(byId.Values, setting);
        }

        public async Task<SyncStateDto> ReplaceAsync(int userId, SyncStateDto incoming)
        {
            Validate(incoming);

            var now = DateTime.UtcNow;
            var habits = await _habitRepository.GetHabitsAsync(userId);
            var byId = habits.ToDictionary(h => h.Id);

            // a replace only carries live habits; tombstones from the device are meaningless here
            var keep = new Dictionary<string, HabitDto>();
            foreach (var dto in incoming.Habits.Where(h => h.DeletedAt == null))
            {
                keep[dto.Id] = dto;
            }

            // Anything the device did not keep is tombstoned rather than removed, so a
            // different device that still has it deletes it on its next sync instead of
            // pushing it back.
            foreach (var existing in byId.Values)
            {
                if (!keep.ContainsKey(existing.Id) && existing.DeletedAt == null)
                {
                    existing.DeletedAt = now;
                    existing.UpdatedAt = now;
                }
            }

            foreach (var dto in keep.Values)
            {
                if (byId.TryGetValue(dto.Id, out var existing))
                {
                    var wasDeleted = existing.DeletedAt != null;
                    Apply(existing, dto);

                    // bring back a tombstoned habit with a fresh clock, otherwise devices
                    // that already saw the deletion would keep treating it as deleted
                    if (wasDeleted) existing.UpdatedAt = now;
                }
                else
                {
                    var habit = new Habit { UserId = userId, Id = dto.Id };
                    Apply(habit, dto);
                    await _habitRepository.AddAsync(habit);
                    byId[habit.Id] = habit;
                }
            }

            var setting = await _habitRepository.GetSettingAsync(userId);
            DateTime? mainAt = incoming.MainHabitUpdatedAt is DateTime m ? ToUtc(m) : null;

            if (setting == null)
            {
                setting = new HabitSetting { UserId = userId };
                await _habitRepository.AddSettingAsync(setting);
            }
            setting.MainHabitId = incoming.MainHabitId;
            setting.MainHabitUpdatedAt = mainAt;

            await _habitRepository.SaveChangesAsync();
            return ToState(byId.Values, setting);
        }

        private static void Validate(SyncStateDto incoming)
        {
            incoming.Habits ??= [];

            if (incoming.Habits.Count > MaxHabits)
            {
                throw new InvalidInputException($"Too many habits (max {MaxHabits})");
            }

            foreach (var dto in incoming.Habits)
            {
                if (string.IsNullOrWhiteSpace(dto.Id) || dto.Id.Length > MaxIdLength)
                {
                    throw new InvalidInputException("Habit id is required and must be at most 64 characters");
                }

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    throw new InvalidInputException("Habit name is required");
                }
            }

            if (incoming.MainHabitId != null && incoming.MainHabitId.Length > MaxIdLength)
            {
                throw new InvalidInputException("Main habit id must be at most 64 characters");
            }
        }

        private static void Apply(Habit habit, HabitDto dto)
        {
            habit.Name = dto.Name;
            habit.StartDate = ToUtc(dto.StartDate);
            habit.Notes = dto.Notes;
            habit.Icon = dto.Icon;
            habit.UpdatedAt = ToUtc(dto.UpdatedAt);
            habit.DeletedAt = dto.DeletedAt is DateTime deletedAt ? ToUtc(deletedAt) : null;
        }

        private static SyncStateDto ToState(IEnumerable<Habit> habits, HabitSetting? setting)
        {
            return new SyncStateDto
            {
                Habits = habits
                    .OrderByDescending(h => h.UpdatedAt)
                    .ThenByDescending(h => h.Id)
                    .Select(h => new HabitDto
                    {
                        Id = h.Id,
                        Name = h.Name,
                        StartDate = h.StartDate,
                        Notes = h.Notes,
                        Icon = h.Icon,
                        UpdatedAt = h.UpdatedAt,
                        DeletedAt = h.DeletedAt
                    })
                    .ToList(),
                MainHabitId = setting?.MainHabitId,
                MainHabitUpdatedAt = setting?.MainHabitUpdatedAt
            };
        }

        // Npgsql only accepts UTC values for timestamptz columns.
        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
