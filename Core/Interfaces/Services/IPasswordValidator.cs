namespace Core.Interfaces.Services
{
    public interface IPasswordValidator
    {
        bool IsValid(string password);
    }
}
