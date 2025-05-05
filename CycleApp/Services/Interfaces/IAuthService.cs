using CycleApp.Contracts.Auth;

namespace CycleApp.Services
{
    public interface IAuthService
    {
        Task<bool> ValidateUserAsync(string email, string code);
        Task<AuthResponse> RegisterUserAsync(RegisterRequest request);
        Task<AuthResponse> LoginUserAsync(string email);
    }
}