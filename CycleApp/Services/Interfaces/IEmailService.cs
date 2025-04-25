namespace CycleApp.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string body);
        Task<bool> ValidateCodeAsync(string email, string code);
    }
}
