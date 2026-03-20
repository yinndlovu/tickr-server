using Core.Entities;

namespace Core.Interfaces.Repositories
{
    public interface IUserAuthRepository
    {
        Task AddAsync(UserAuth userAuth);
        Task UpdateAsync(UserAuth userAuth);
        Task<UserAuth?> FindUserAuthByEmail(string email);
        Task<UserAuth?> FindByProviderAndProviderId(string provider, string providerUserId);
        Task<bool> SaveChangesAsync();
    }
}
