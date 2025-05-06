using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CycleApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly CycleDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            CycleDbContext dbContext,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendPeriodReminderAsync(int userId, DateTime expectedDate)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null || !user.RemindPeriod)
            {
                _logger.LogInformation($"Пропуск напоминания о менструации для пользователя {userId}: пользователь не найден или уведомления отключены");
                return;
            }
                
            var daysUntil = (expectedDate - DateTime.UtcNow).Days;
            
            if (daysUntil <= 0)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Менструация должна начаться сегодня",
                    "По нашим расчетам, ваша менструация должна начаться сегодня.");
                    
                _logger.LogInformation($"Отправлено напоминание о начале менструации сегодня для пользователя {userId}");
            }
            else if (daysUntil <= 3)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    $"Скоро начнется менструация",
                    $"По нашим расчетам, ваша менструация должна начаться через {daysUntil} дней ({expectedDate.ToShortDateString()}).");
                    
                _logger.LogInformation($"Отправлено предварительное напоминание о менструации для пользователя {userId} (через {daysUntil} дней)");
            }
        }

        public async Task SendOvulationReminderAsync(int userId, DateTime expectedDate)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null || !user.RemindOvulation)
            {
                _logger.LogInformation($"Пропуск напоминания об овуляции для пользователя {userId}: пользователь не найден или уведомления отключены");
                return;
            }
                
            var daysUntil = (expectedDate - DateTime.UtcNow).Days;
            
            if (daysUntil <= 0)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Овуляция сегодня",
                    "По нашим расчетам, ваша овуляция должна быть сегодня.");
                    
                _logger.LogInformation($"Отправлено напоминание об овуляции сегодня для пользователя {userId}");
            }
            else if (daysUntil <= 2)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    $"Скоро овуляция",
                    $"По нашим расчетам, ваша овуляция должна быть через {daysUntil} дней ({expectedDate.ToShortDateString()}).");
                    
                _logger.LogInformation($"Отправлено предварительное напоминание об овуляции для пользователя {userId} (через {daysUntil} дней)");
            }
        }

        public async Task<bool> ScheduleNotifications(int userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                _logger.LogWarning($"Не удалось запланировать уведомления: пользователь {userId} не найден");
                return false;
            }
            
            // В реальном приложении здесь может быть логика планирования уведомлений
            // через внешние сервисы уведомлений или push-notifications
            
            _logger.LogInformation($"Уведомления запланированы для пользователя {userId}");
            return true;
        }

        public async Task<bool> CancelNotifications(int userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                _logger.LogWarning($"Не удалось отменить уведомления: пользователь {userId} не найден");
                return false;
            }
            
            // В реальном приложении здесь может быть логика отмены уведомлений
            
            _logger.LogInformation($"Уведомления отменены для пользователя {userId}");
            return true;
        }
    }
}
