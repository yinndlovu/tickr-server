using Core.Entities;

namespace Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task<User?> FindByEmail(string email);
        Task<User?> FindById(int id);
        Task<bool> UserEmailExists(string email);
        Task<bool> SaveChangesAsync();
    }
}
