using Microsoft.AspNetCore.Mvc;
using CycleApp.Services.Interfaces;
using System.Threading.Tasks;

namespace CycleApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailVerificationController : ControllerBase
    {
        private readonly IEmailService _email;

        public EmailVerificationController(IEmailService email)
        {
            _email = email;
        }

        [HttpPost("send-code")]
        public async Task<IActionResult> SendCode([FromQuery] string email)
        {
            await _email.SendVerificationCodeAsync(email);
            return Accepted(); // 202
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromQuery] string email, [FromQuery] string code)
        {
            if (await _email.ValidateCodeAsync(email, code))
                return Ok(); // 200
            return BadRequest("Invalid or expired code.");
        }
    }
}
