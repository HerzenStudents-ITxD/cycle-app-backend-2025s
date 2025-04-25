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
        private readonly CycleDbContext _dbContext;

        public AuthService(ITokenService tokenService, IEmailService emailService, CycleDbContext dbContext)
        {
            _tokenService = tokenService;
            _emailService = emailService;
            _dbContext = dbContext;
        }

        public async Task<bool> ValidateUserAsync(string email, string code)
        {

            var isValid = await _emailService.ValidateCodeAsync(email, code);
            return isValid;
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
                CycleLength = request.CycleLength,
                PeriodLength = request.PeriodLength,
                RemindPeriod = true,
                RemindOvulation = true
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();


            var token = _tokenService.GenerateToken(user);
            return new AuthResponse(token, false, user.Email, user.UserId);
        }
    }
}