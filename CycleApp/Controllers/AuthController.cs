using CycleApp.Contracts.Auth;
using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Services;
using CycleApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CycleApp.Controllers
{
    [AllowAnonymous]
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

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] [Required] [EmailAddress] string email)
        {
            try
            {
                // Используем более безопасный способ создания случайного кода
                var code = new Random(Guid.NewGuid().GetHashCode()).Next(100000, 999999).ToString();
                var expiration = TimeSpan.FromMinutes(15);
                _codeStorage.StoreCode(email, code, expiration);

                // Check if user exists
                var userExists = _dbContext.Users.Any(u => u.Email == email);
                string responseMessage;

                if (userExists)
                {
                    responseMessage = "Login code sent to your email";
                }
                else
                {
                    responseMessage =
                        "Registration code sent to your email. You'll need to complete registration after verification.";
                }

                // Всегда отправляем код по электронной почте
                try
                {
                    await _emailService.SendEmailAsync(email, "Your Authentication Code", $"Your code is: {code}");
                    _logger.LogInformation("Email sent successfully to {Email}", email);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send email to {Email}, but continuing with authentication process", email);
                }
        
                // В режиме разработки дополнительно логируем код
                if (_env.IsDevelopment())
                {
                    _logger.LogInformation("Development mode authentication code for {Email}: {Code}", email, code);
                    return Ok(new
                    {
                        message = responseMessage,
                        isNewUser = !userExists,
                        code
                    });
                }
                else
                {
                    return Ok(new { message = responseMessage, isNewUser = !userExists });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending authentication code to {Email}", email);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
            _logger.LogInformation("Verifying code {Code} for {Email}", request.Code, request.Email);

            bool isCodeValid = await _authService.ValidateUserAsync(request.Email, request.Code);
            if (!isCodeValid)
            {
                _logger.LogError("Invalid verification code for {Email}", request.Email);
                return BadRequest(new { message = "Invalid or expired code" });
            }

            // Check if user exists
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user != null)
            {
                // Existing user login flow
                var token = _tokenService.GenerateToken(user);
                _logger.LogInformation("JWT token generated for existing user {Email}", request.Email);
                return Ok(new AuthResponse(token, false, user.Email, user.UserId));
            }
            else
            {
                // New user registration flow - return a special token
                var token = _tokenService.GenerateToken(request.Email);
                _logger.LogInformation("Registration token generated for new user {Email}", request.Email);
                return Ok(new AuthResponse(token, true, request.Email));
            }
        }

        [HttpPost("complete-registration")]
        public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationRequest request)
        {
            // Validate the registration token
            if (!_tokenService.ValidateToken(request.Token))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            // Extract email from token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(request.Token);
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);

            if (emailClaim == null)
            {
                return BadRequest(new { message = "Invalid token format" });
            }

            string email = emailClaim.Value;

            // Check if user already exists
            if (_dbContext.Users.Any(u => u.Email == email))
            {
                return BadRequest(new { message = "User already exists" });
            }

            // Create the new user
            var user = new User
            {
                Email = email,
                CycleLength = request.CycleLength > 0 ? request.CycleLength : 28,
                PeriodLength = request.PeriodLength > 0 ? request.PeriodLength : 5,
                RemindPeriod = true,
                RemindOvulation = true
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Generate a full user token
            var userToken = _tokenService.GenerateToken(user);
            _logger.LogInformation("User registration completed for {Email}", email);

            return Ok(new AuthResponse(userToken, false, email, user.UserId));
        }
    }
}