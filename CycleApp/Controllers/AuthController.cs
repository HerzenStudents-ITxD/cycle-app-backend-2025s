using CycleApp.Contracts.Auth;
using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Models.Auth;
using CycleApp.Services;
using CycleApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;

namespace CycleApp.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(
        IAuthService authService,
        ICodeStorageService codeStorage,
        IEmailService emailService,
        ITokenService tokenService,
        CycleDbContext dbContext,
        ILogger<AuthController> logger,
        IWebHostEnvironment env) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly ICodeStorageService _codeStorage = codeStorage;
        private readonly IEmailService _emailService = emailService;
        private readonly ITokenService _tokenService = tokenService;
        private readonly CycleDbContext _dbContext = dbContext;
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IWebHostEnvironment _env = env;

        [HttpPost("send-code")]
        public async Task<IActionResult> SendCode([FromBody][Required][EmailAddress] string email)
        {
            try
            {
                var code = new Random().Next(100000, 999999).ToString();
                var expiration = TimeSpan.FromMinutes(15);

                _codeStorage.StoreCode(email, code, expiration);

                if (!_env.IsDevelopment())
                {
                    await _emailService.SendEmailAsync(email, "Your Verification Code", $"Your code is: {code}");
                }
                else
                {
                    _logger.LogInformation("В режиме разработки. Код подтверждения для {Email}: {Code}", email, code);
                    return Ok(new
                    {
                        message = "Verification code generated (development mode)",
                        code
                    });
                }

                return Ok(new { message = "Verification code sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке кода подтверждения на {Email}", email);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("verify-code")]
        public IActionResult VerifyCode([FromBody] VerifyCodeRequest request)
        {
            _logger.LogInformation("Попытка верификации кода {Code} для {Email}", request.Code, request.Email);

            if (!_codeStorage.ValidateCode(request.Email, request.Code))
            {
                _logger.LogError("Ошибка верификации кода для {Email}", request.Email);
                return BadRequest(new { message = "Invalid or expired code" });
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
            {
                var tempToken = _tokenService.GenerateTempToken(request.Email);
                _logger.LogInformation("Сгенерирован временный токен для {Email}", request.Email);
                return Ok(new AuthResponse(tempToken, true, request.Email));
            }

            var token = _tokenService.GenerateToken(user);
            _logger.LogInformation("Сгенерирован JWT токен для {Email}", user.Email);
            return Ok(new AuthResponse(token, false, user.Email, user.UserId));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!_tokenService.ValidateTempToken(request.TempToken, request.Email))
                return Unauthorized();

            if (_dbContext.Users.Any(u => u.Email == request.Email))
                return BadRequest(new { message = "User already exists" });

            var CycleLength = request.CycleLength;
            if (!(CycleLength > 0))
            {
                CycleLength = 28;
            }

            var PeriodLength = request.PeriodLength;
            if(!(PeriodLength > 0))
            {
                PeriodLength = 5;
            }

            var user = new User
            {
                Email = request.Email,
                CycleLength = CycleLength,
                PeriodLength = PeriodLength,
                RemindPeriod = true,
                RemindOvulation = true
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);
            return Ok(new AuthResponse(token, false, user.Email, user.UserId));
        }
    }
}