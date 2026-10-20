using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserAuth> UserAuths { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitSetting> HabitSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserAuth>()
                .HasIndex(a => new { a.Provider, a.ProviderUserId })
                .IsUnique();

            modelBuilder.Entity<UserAuth>()
                .HasIndex(a => a.NormalizedEmail)
                .IsUnique()
                .HasFilter("\"Provider\" = 'local'");

            modelBuilder.Entity<UserAuth>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.AuthMethods)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSession>()
                .HasIndex(s => s.ExpiresAt);

            modelBuilder.Entity<UserSession>()
                .HasIndex(s => s.UserId);

            modelBuilder.Entity<UserSession>()
                .HasIndex(s => s.RefreshTokenHash)
                .IsUnique();

            modelBuilder.Entity<UserSession>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // habits are keyed by (user, client-generated id) so devices can create them offline
            modelBuilder.Entity<Habit>(habit =>
            {
                habit.HasKey(h => new { h.UserId, h.Id });
                habit.Property(h => h.Id).HasMaxLength(64);
                habit.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(h => h.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<HabitSetting>(setting =>
            {
                setting.HasKey(s => s.UserId);
                setting.Property(s => s.MainHabitId).HasMaxLength(64);
                setting.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<HabitSetting>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
