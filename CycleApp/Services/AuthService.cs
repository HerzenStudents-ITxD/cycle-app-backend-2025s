using CycleApp.Contracts.Auth;
using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CycleApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ICodeStorageService _codeStorage;
        private readonly CycleDbContext _dbContext;
        
        public AuthService(ITokenService tokenService, IEmailService emailService, ICodeStorageService codeStorage, CycleDbContext dbContext)
        {
            _tokenService = tokenService;
            _emailService = emailService;
            _codeStorage = codeStorage;
            _dbContext = dbContext;
        }

        public async Task<bool> ValidateUserAsync(string email, string code)
        {
            // Используем CodeStorageService для проверки кода
            var isValid = _codeStorage.ValidateCode(email, code);
            return await Task.FromResult(isValid);
        }

        public async Task<AuthResponse> RegisterUserAsync(RegisterRequest request)
        {
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return new AuthResponse(null, false, request.Email);
            }

            var user = new User
            {
                Email = request.Email,
                CycleLength = request.CycleLength > 0 ? request.CycleLength : 28,
                PeriodLength = request.PeriodLength > 0 ? request.PeriodLength : 5,
                RemindPeriod = true,
                RemindOvulation = true
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);
            return new AuthResponse(token, false, user.Email, user.UserId);
        }

        public async Task<AuthResponse> LoginUserAsync(string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                // Return token for new user flow
                var token = _tokenService.GenerateToken(email);
                return new AuthResponse(token, true, email);
            }

            // Return token for existing user
            var userToken = _tokenService.GenerateToken(user);
            return new AuthResponse(userToken, false, user.Email, user.UserId);
        }
    }
}