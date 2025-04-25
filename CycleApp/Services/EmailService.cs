using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using CycleApp.Services.Interfaces;

namespace CycleApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var fromAddress = _configuration["Email:FromAddress"] ?? throw new ArgumentNullException("Email:FromAddress не настроен");
                var smtpServer = _configuration["Email:SmtpServer"] ?? throw new ArgumentNullException("Email:SmtpServer не настроен");
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? throw new ArgumentNullException("Email:SmtpPort не настроен"));
                var smtpUser = _configuration["Email:SmtpUser"] ?? throw new ArgumentNullException("Email:SmtpUser не настроен");
                var smtpPass = _configuration["Email:SmtpPass"] ?? throw new ArgumentNullException("Email:SmtpPass не настроен");

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("CycleApp", fromAddress));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = body };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(smtpUser, smtpPass);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке email на {To}", to);
                throw;
            }
        }

        public async Task<bool> ValidateCodeAsync(string email, string code)
        {
            // Implementation to validate the code
            // This is a placeholder implementation
            // Replace with actual validation logic
            return code == "123456"; // Example validation
        }
    }
}
