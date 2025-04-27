using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using CycleApp.DataAccess;
using CycleApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleApp.Services
{
    public class CycleCalculationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CycleCalculationBackgroundService> _logger;

        public CycleCalculationBackgroundService(
            IServiceProvider services,
            ILogger<CycleCalculationBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cycle Calculation Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cycle Calculation Service is processing.");

                using (var scope = _services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CycleDbContext>();
                    var calculator = scope.ServiceProvider.GetRequiredService<ICycleCalculatorService>();

                    var users = await dbContext.Users
                        .Include(u => u.Periods)
                        .Include(u => u.Ovulations)
                        .ToListAsync(stoppingToken);

                    foreach (var user in users)
                    {
                        try
                        {
                            // Рассчёт следующих периодов и овуляций
                            var (ovulationStart, ovulationEnd) = calculator.CalculateNextOvulation(user);
                            var (periodStart, periodEnd) = calculator.CalculateNextPeriod(user);

                            var existingOvulation = await dbContext.Ovulations
                                .FirstOrDefaultAsync(o =>
                                    o.UserId == user.UserId &&
                                    o.StartDate >= DateTime.UtcNow,
                                    stoppingToken);

                            if (existingOvulation == null)
                            {
                                dbContext.Ovulations.Add(new Ovulation
                                {
                                    UserId = user.UserId,   
                                    StartDate = ovulationStart,
                                    EndDate = ovulationEnd,
                                    IsPredicted = true
                                });
                            }

                            var existingPeriod = await dbContext.Periods
                                .FirstOrDefaultAsync(p => p.UserId == user.UserId && p.StartDate >= DateTime.UtcNow);

                            if (existingPeriod == null)
                            {
                                dbContext.Periods.Add(new Period
                                {
                                    UserId = user.UserId,
                                    StartDate = periodStart,
                                    EndDate = periodEnd,
                                    IsActive = false,
                                    IsPredicted = true
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing user {UserId}", user.UserId);
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("Cycle Calculation Service is stopping.");
        }
    }
}