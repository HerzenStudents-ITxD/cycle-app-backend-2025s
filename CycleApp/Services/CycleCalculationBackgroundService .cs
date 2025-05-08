using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using CycleApp.DataAccess;
using CycleApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CycleApp.Services
{
    public class CycleCalculationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CycleCalculationBackgroundService> _logger;
        private const int CYCLES_TO_PREDICT = 3; // Predict 3 cycles ahead
        private const int OVULATION_WINDOW_DAYS = 3; // Ovulation window is 3 days
        private const int MAX_RETRIES = 3;
        private const int RETRY_DELAY_MS = 1000;

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
                        await ProcessUserWithRetry(user, dbContext, calculator, stoppingToken);
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("Cycle Calculation Service is stopping.");
        }

        internal async Task ProcessUserWithRetry(User user, CycleDbContext dbContext, ICycleCalculatorService calculator, CancellationToken stoppingToken)
        {
            int retryCount = 0;
            while (retryCount < MAX_RETRIES)
            {
                try
                {
                    await ProcessUser(user, dbContext, calculator, stoppingToken);
                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogError(ex, "Error processing user {UserId} (Attempt {RetryCount}/{MaxRetries})", 
                        user.UserId, retryCount, MAX_RETRIES);

                    if (retryCount < MAX_RETRIES)
                    {
                        await Task.Delay(RETRY_DELAY_MS * retryCount, stoppingToken);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private async Task ProcessUser(User user, CycleDbContext dbContext, ICycleCalculatorService calculator, CancellationToken stoppingToken)
        {
            // Update cycle variations if needed
            if (!user.LastCycleVariationUpdate.HasValue || 
                (DateTime.UtcNow - user.LastCycleVariationUpdate.Value).TotalDays >= 7)
            {
                calculator.UpdateCycleVariations(user);
            }

            // Clean up old predictions
            var oldPredictions = await dbContext.Periods
                .Where(p => p.UserId == user.UserId && 
                          p.IsPredicted && 
                          p.StartDate < DateTime.UtcNow)
                .ToListAsync(stoppingToken);
            
            dbContext.Periods.RemoveRange(oldPredictions);

            var oldOvulationPredictions = await dbContext.Ovulations
                .Where(o => o.UserId == user.UserId && 
                          o.IsPredicted && 
                          o.StartDate < DateTime.UtcNow)
                .ToListAsync(stoppingToken);
            
            dbContext.Ovulations.RemoveRange(oldOvulationPredictions);

            // Get the last actual period
            var lastActualPeriod = user.Periods
                .Where(p => !p.IsPredicted)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            DateTime baseDate = lastActualPeriod?.StartDate ?? DateTime.UtcNow;

            // Predict multiple cycles ahead
            for (int i = 0; i < CYCLES_TO_PREDICT; i++)
            {
                var (ovulationStart, ovulationEnd) = calculator.CalculateNextOvulation(user, baseDate);
                var (periodStart, periodEnd) = calculator.CalculateNextPeriod(user, baseDate);

                // Check if we already have predictions for these dates
                var existingOvulation = await dbContext.Ovulations
                    .FirstOrDefaultAsync(o =>
                        o.UserId == user.UserId &&
                        o.StartDate == ovulationStart &&
                        o.IsPredicted,
                        stoppingToken);

                if (existingOvulation == null)
                {
                    dbContext.Ovulations.Add(new Ovulation
                    {
                        UserId = user.UserId,
                        StartDate = ovulationStart,
                        EndDate = ovulationStart.AddDays(OVULATION_WINDOW_DAYS),
                        IsPredicted = true
                    });
                }

                var existingPeriod = await dbContext.Periods
                    .FirstOrDefaultAsync(p => 
                        p.UserId == user.UserId && 
                        p.StartDate == periodStart &&
                        p.IsPredicted);

                if (existingPeriod == null)
                {
                    dbContext.Periods.Add(new Period
                    {
                        UserId = user.UserId,
                        StartDate = periodStart,
                        EndDate = periodEnd,
                        IsActive = false,
                        IsPredicted = true,
                        DayOfCycle = (i + 1) * user.CycleLength
                    });
                }

                // Update base date for next iteration
                baseDate = periodStart;
            }
        }
    }
}