using Core.Interfaces.Services;
using System.Text.RegularExpressions;

namespace Application.Auth.Services
{
    public partial class PasswordValidator : IPasswordValidator
    {
        public bool IsValid(string password)
        {
            var regex = PasswordRegex();
            bool isValid = regex.IsMatch(password);
            return isValid;
        }

        [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,}$")]
        private static partial Regex PasswordRegex();
    }
}
