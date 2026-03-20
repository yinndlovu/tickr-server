namespace Core.Interfaces.Repositories
{
    public interface IAppRepository
    {
        Task<bool> SaveChangesAsync();
    }
}
