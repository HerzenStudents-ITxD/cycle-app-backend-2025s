//using CycleApp.Contracts.Auth;
//using CycleApp.DataAccess;
//using CycleApp.Models;
//using CycleApp.Models.Auth;
//using CycleApp.Services;
//using Microsoft.AspNetCore.Identity.Data;
//using Microsoft.AspNetCore.Mvc;
//using System.ComponentModel.DataAnnotations;
//using CycleApp.Services.Interfaces;
//using MyRegisterRequest = CycleApp.Contracts.Auth.RegisterRequest;

//namespace CycleApp.Controllers
//{
//    [ApiController]
//    [Route("api/auth")]
//    public class AuthController : ControllerBase
//    {
//        private readonly IAuthService _authService;
//        private readonly ICodeStorageService _codeStorage;
//        private readonly IEmailService _emailService;
//        private readonly ITokenService _tokenService;
//        private readonly CycleDbContext _dbContext;

//        public AuthController(
//            IAuthService authService,
//            ICodeStorageService codeStorage,
//            IEmailService emailService,
//            ITokenService tokenService,
//            CycleDbContext dbContext)
//        {
//            _authService = authService;
//            _codeStorage = codeStorage;
//            _emailService = emailService;
//            _tokenService = tokenService;
//            _dbContext = dbContext;
//        }

//        [HttpPost("send-code")]
//        public async Task<IActionResult> SendCode([FromBody][Required][EmailAddress] string email)
//        {
//            var code = new Random().Next(100000, 999999).ToString();
//            var expiration = TimeSpan.FromMinutes(15);

//            _codeStorage.StoreCode(email, code, expiration);

//            await _emailService.SendEmailAsync(email, "Your Verification Code", $"Your code is: {code}");

//            return Ok(new { message = "Verification code sent" });
//        }

//        [HttpPost("verify-code")]
//        public IActionResult VerifyCode([FromBody] VerifyCodeRequest request)
//        {
//            if (!_codeStorage.ValidateCode(request.Email, request.Code))
//                return BadRequest(new { message = "Invalid or expired code" });

//            var user = _dbContext.Users.FirstOrDefault(u => u.Email == request.Email);

//            if (user == null)
//            {
//                var tempToken = _tokenService.GenerateTempToken(request.Email);
//                return Ok(new AuthResponse(tempToken, true, request.Email));
//            }

//            var token = _tokenService.GenerateToken(user);
//            return Ok(new AuthResponse(token, false, user.Email, user.UserId));
//        }

//        [HttpPost("register")]
//        public async Task<IActionResult> Register([FromBody] MyRegisterRequest request)
//        {
//            if (!_tokenService.ValidateTempToken(request.TempToken, request.Email))
//                return Unauthorized();

//            if (_dbContext.Users.Any(u => u.Email == request.Email))
//                return BadRequest(new { message = "User already exists" });

//            var user = new User
//            {
//                Email = request.Email,
//                CycleLength = request.CycleLength,
//                PeriodLength = request.PeriodLength,
//                RemindPeriod = true,
//                RemindOvulation = true
//            };

//            _dbContext.Users.Add(user);
//            await _dbContext.SaveChangesAsync();

//            var token = _tokenService.GenerateToken(user);
//            return Ok(new AuthResponse(token, false, user.Email, user.UserId));
//        }
//    }
//}
using CycleApp.Contracts.Auth;
using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Models.Auth;
using CycleApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using MyRegisterRequest = CycleApp.Contracts.Auth.RegisterRequest;
using CycleApp.Services;
using CycleApp.Services.Interfaces;

namespace CycleApp.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICodeStorageService _codeStorage;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly CycleDbContext _dbContext;
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _env;

        public AuthController(
            IAuthService authService,
            ICodeStorageService codeStorage,
            IEmailService emailService,
            ITokenService tokenService,
            CycleDbContext dbContext,
            ILogger<AuthController> logger,
            IWebHostEnvironment env)
        {
            _authService = authService;
            _codeStorage = codeStorage;
            _emailService = emailService;
            _tokenService = tokenService;
            _dbContext = dbContext;
            _logger = logger;
            _env = env;
        }

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
                        code = code 
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
            _logger.LogInformation($"Попытка верификации кода {request.Code} для {request.Email}");

            if (!_codeStorage.ValidateCode(request.Email, request.Code))
            {
                _logger.LogError($"Ошибка верификации кода для {request.Email}");
                return BadRequest(new { message = "Invalid or expired code" });
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
            {
                var tempToken = _tokenService.GenerateTempToken(request.Email);
                _logger.LogInformation($"Сгенерирован временный токен для {request.Email}");
                return Ok(new AuthResponse(tempToken, true, request.Email));
            }

            var token = _tokenService.GenerateToken(user);
            _logger.LogInformation($"Сгенерирован JWT токен для {user.Email}");
            return Ok(new AuthResponse(token, false, user.Email, user.UserId));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] MyRegisterRequest request)
        {
            if (!_tokenService.ValidateTempToken(request.TempToken, request.Email))
                return Unauthorized();

            if (_dbContext.Users.Any(u => u.Email == request.Email))
                return BadRequest(new { message = "User already exists" });

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
            return Ok(new AuthResponse(token, false, user.Email, user.UserId));
        }
    }
}