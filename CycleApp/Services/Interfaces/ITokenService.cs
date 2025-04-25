using CycleApp.Models;

namespace CycleApp.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        string GenerateTempToken(string email);
        bool ValidateTempToken(string token, string email);
    }
}