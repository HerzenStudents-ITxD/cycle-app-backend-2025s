using CycleApp.Models;

namespace CycleApp.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        string GenerateToken(string email);
        bool ValidateToken(string token);
        int GetUserIdFromToken(string token);
        string GetEmailFromToken(string token);
    }
}