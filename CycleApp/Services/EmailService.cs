using Microsoft.Extensions.Caching.Memory;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using CycleApp.Services.Interfaces;
using System.Net.Sockets;

namespace CycleApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly ICodeStorageService _codeStorage;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            ICodeStorageService codeStorage)
        {
            _configuration = configuration;
            _logger = logger;
            _codeStorage = codeStorage;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Проверка и получение настроек электронной почты
                var fromAddress = _configuration["Email:FromAddress"];
                if (string.IsNullOrEmpty(fromAddress))
                {
                    _logger.LogError("Email:FromAddress is not configured");
                    throw new ArgumentNullException("Email:FromAddress is not configured");
                }
        
                var smtpServer = _configuration["Email:SmtpServer"];
                if (string.IsNullOrEmpty(smtpServer))
                {
                    _logger.LogError("Email:SmtpServer is not configured");
                    throw new ArgumentNullException("Email:SmtpServer is not configured");
                }
        
                var smtpPortStr = _configuration["Email:SmtpPort"];
                if (string.IsNullOrEmpty(smtpPortStr) || !int.TryParse(smtpPortStr, out int smtpPort))
                {
                    _logger.LogError("Email:SmtpPort is not configured or not a valid number");
                    throw new ArgumentNullException("Email:SmtpPort is not configured or not a valid number");
                }
        
                var smtpUser = _configuration["Email:SmtpUser"];
                if (string.IsNullOrEmpty(smtpUser))
                {
                    _logger.LogError("Email:SmtpUser is not configured");
                    throw new ArgumentNullException("Email:SmtpUser is not configured");
                }
        
                var smtpPass = _configuration["Email:SmtpPass"];
                if (string.IsNullOrEmpty(smtpPass))
                {
                    _logger.LogError("Email:SmtpPass is not configured");
                    throw new ArgumentNullException("Email:SmtpPass is not configured");
                }
        
                // Создание сообщения
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("CycleApp", fromAddress));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
        
                var builder = new BodyBuilder { HtmlBody = body };
                email.Body = builder.ToMessageBody();
        
                // Отправка сообщения
                using var smtp = new SmtpClient();
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true; // For testing purposes only
                smtp.Timeout = 30000; // Увеличиваем таймаут до 30 секунд
        
                _logger.LogInformation("Connecting to SMTP server {Server}:{Port}", smtpServer, smtpPort);
                try
                {
                    await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                }
                catch (SocketException ex)
                {
                    _logger.LogError(ex, "Could not resolve SMTP host {Server}", smtpServer);
                    throw new InvalidOperationException($"SMTP host '{smtpServer}' could not be resolved. Check your Email:SmtpServer setting.", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to connect to SMTP server {Server}:{Port}", smtpServer, smtpPort);
                    throw new InvalidOperationException($"Failed to connect to SMTP server: {ex.Message}", ex);
                }
        
                try
                {
                    _logger.LogInformation("Authenticating as {User}", smtpUser);
                    await smtp.AuthenticateAsync(smtpUser, smtpPass);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Authentication failed for user {User}", smtpUser);
                    throw new InvalidOperationException($"SMTP authentication failed. Check your Email:SmtpUser and Email:SmtpPass settings.", ex);
                }
        
                try
                {
                    _logger.LogInformation("Sending email to {Recipient}", to);
                    await smtp.SendAsync(email);
                    
                    _logger.LogInformation("Disconnecting from SMTP server");
                    await smtp.DisconnectAsync(true);
                    
                    _logger.LogInformation("Email sent successfully to {Recipient}", to);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {Recipient}", to);
                    throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Recipient}", to);
                throw; // Перепрокидываем исключение для обработки на уровне выше
            }
        }

        public async Task SendVerificationCodeAsync(string toEmail)
        {
            var code = new Random().Next(100000, 999999).ToString();
            _codeStorage.StoreCode(toEmail, code, TimeSpan.FromMinutes(5));

            var subject = "Your Verification Code";
            var body = $@"
                <p>Hello,</p>
                <p>Your verification code is: <strong>{code}</strong></p>
                <p>Please enter this code to verify your email address.</p>";

            _logger.LogInformation("Sending verification code to {Email}", toEmail);
            await SendEmailAsync(toEmail, subject, body);
        }

        public Task<bool> ValidateCodeAsync(string email, string code)
        {
            var isValid = _codeStorage.ValidateCode(email, code);
            return Task.FromResult(isValid);
        }
    }
}
