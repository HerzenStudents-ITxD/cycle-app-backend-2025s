using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CycleApp.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting notification check process");

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<CycleDbContext>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        var cycleCalculatorService = scope.ServiceProvider.GetRequiredService<ICycleCalculatorService>();

                        // Получаем пользователей с включенными уведомлениями
                        var users = await dbContext.Users
                            .Where(u => u.RemindPeriod || u.RemindOvulation)
                            .ToListAsync(stoppingToken);

                        foreach (var user in users)
                        {
                            if (stoppingToken.IsCancellationRequested)
                                break;

                            try
                            {
                                // Проверяем, нужно ли отправить уведомление о менструации
                                if (user.RemindPeriod)
                                {
                                    var nextPeriodDate = await cycleCalculatorService.CalculateNextPeriod(user.UserId);
                                    var daysUntilPeriod = (nextPeriodDate - DateTime.UtcNow).Days;

                                    if (daysUntilPeriod <= 3 && daysUntilPeriod >= 0)
                                    {
                                        await notificationService.SendPeriodReminderAsync(user.UserId, nextPeriodDate);
                                    }
                                }

                                // Проверяем, нужно ли отправить уведомление об овуляции
                                if (user.RemindOvulation)
                                {
                                    var nextPeriodDate = await cycleCalculatorService.CalculateNextPeriod(user.UserId);
                                    var ovulationDate = nextPeriodDate.AddDays(-14); // Примерно за 14 дней до следующей менструации
                                    var daysUntilOvulation = (ovulationDate - DateTime.UtcNow).Days;

                                    if (daysUntilOvulation <= 2 && daysUntilOvulation >= 0)
                                    {
                                        await notificationService.SendOvulationReminderAsync(user.UserId, ovulationDate);
                                    }
                                }
                            }
                            catch (Exception userEx)
                            {
                                _logger.LogError(userEx, "Error processing notifications for user {UserId}", user.UserId);
                                // Продолжаем с следующим пользователем
                            }
                        }
                    }

                    _logger.LogInformation("Notification check completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in notification background service");
                }

                // Ждем 6 часов перед следующей проверкой
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }
}
