using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository(AppDbContext dbContext) : IUserRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task AddAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
        }

        public async Task<User?> FindByEmail(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var userAuth = await _dbContext.UserAuths
                .Include(ua => ua.User)
                .FirstOrDefaultAsync(ua => ua.NormalizedEmail == normalizedEmail
                && ua.Provider == AuthProvider.Local.ToString().ToLowerInvariant());

            return userAuth?.User;
        }

        public async Task<User?> FindById(int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<bool> UserEmailExists(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var exists = await _dbContext.UserAuths
                .Include(ua => ua.User)
                .AnyAsync(ua => ua.NormalizedEmail == normalizedEmail
                && ua.Provider == AuthProvider.Local.ToString().ToLowerInvariant());

            return exists;
        }

        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                var rows = await _dbContext.SaveChangesAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
