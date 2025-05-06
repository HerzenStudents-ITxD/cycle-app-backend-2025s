using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CycleApp.Services
{
    public class CycleAnalyticsService : ICycleAnalyticsService
    {
        private readonly CycleDbContext _dbContext;
        private readonly ILogger<CycleAnalyticsService> _logger;

        public CycleAnalyticsService(CycleDbContext dbContext, ILogger<CycleAnalyticsService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<object> GetCycleAnalytics(int userId, int cyclesCount = 6)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                _logger.LogWarning($"Пользователь с ID {userId} не найден при получении аналитики цикла");
                return null;
            }
                
            var periods = await _dbContext.Periods
                .Where(p => p.UserId == userId && p.EndDate != null)
                .OrderByDescending(p => p.StartDate)
                .Take(cyclesCount)
                .ToListAsync();
                
            var avgCycleDuration = await GetAverageCycleDuration(userId, cyclesCount);
            var avgPeriodDuration = await GetAveragePeriodDuration(userId, cyclesCount);
            var regularity = await GetRegularityAnalysis(userId, cyclesCount);
            
            var nextPeriodDate = PredictNextPeriod(periods, user);
            var nextOvulationDate = PredictNextOvulation(periods, user);
                
            var analytics = new
            {
                AverageCycleLength = avgCycleDuration,
                AveragePeriodLength = avgPeriodDuration,
                Regularity = regularity,
                PreviousPeriods = periods,
                PredictedNextPeriod = nextPeriodDate,
                PredictedNextOvulation = nextOvulationDate
            };
            
            return analytics;
        }

        public async Task<object> GetAverageCycleDuration(int userId, int cyclesCount = 6)
        {
            var periods = await _dbContext.Periods
                .Where(p => p.UserId == userId && p.EndDate != null)
                .OrderByDescending(p => p.StartDate)
                .Take(cyclesCount + 1) // Нужно на один больше для расчета цикла
                .ToListAsync();
                
            if (periods.Count < 2)
            {
                _logger.LogInformation($"Недостаточно данных для расчета средней продолжительности цикла для пользователя {userId}");
                return null;
            }
                
            var cycleLengths = new List<int>();
            for (int i = 0; i < periods.Count - 1; i++)
            {
                var daysInCycle = (int)(periods[i].StartDate - periods[i + 1].EndDate.Value).TotalDays;
                if (daysInCycle > 0 && daysInCycle < 100) // Защита от некорректных данных
                    cycleLengths.Add(daysInCycle);
            }
            
            if (cycleLengths.Count == 0)
            {
                _logger.LogWarning($"Не удалось рассчитать продолжительность цикла для пользователя {userId}");
                return null;
            }
                
            return new
            {
                AverageDays = cycleLengths.Average(),
                MinDays = cycleLengths.Min(),
                MaxDays = cycleLengths.Max(),
                CyclesAnalyzed = cycleLengths.Count
            };
        }

        public async Task<object> GetAveragePeriodDuration(int userId, int cyclesCount = 6)
        {
            var periods = await _dbContext.Periods
                .Where(p => p.UserId == userId && p.EndDate != null)
                .OrderByDescending(p => p.StartDate)
                .Take(cyclesCount)
                .ToListAsync();
                
            if (periods.Count == 0)
            {
                _logger.LogInformation($"Нет данных о менструациях для пользователя {userId}");
                return null;
            }
                
            var periodLengths = periods
                .Where(p => p.EndDate.HasValue)
                .Select(p => (int)(p.EndDate.Value - p.StartDate).TotalDays + 1)
                .Where(days => days > 0 && days < 20) // Защита от некорректных данных
                .ToList();
                
            if (periodLengths.Count == 0)
            {
                _logger.LogWarning($"Не удалось рассчитать продолжительность менструаций для пользователя {userId}");
                return null;
            }
                
            return new
            {
                AverageDays = periodLengths.Average(),
                MinDays = periodLengths.Min(),
                MaxDays = periodLengths.Max(),
                PeriodsAnalyzed = periodLengths.Count
            };
        }

        public async Task<object> GetRegularityAnalysis(int userId, int cyclesCount = 6)
        {
            var avgCycleResult = await GetAverageCycleDuration(userId, cyclesCount) as dynamic;
            if (avgCycleResult == null)
            {
                _logger.LogInformation($"Недостаточно данных для анализа регулярности для пользователя {userId}");
                return null;
            }
                
            double averageCycle = avgCycleResult.AverageDays;
            double minCycle = avgCycleResult.MinDays;
            double maxCycle = avgCycleResult.MaxDays;
            
            // Определение регулярности
            double variation = maxCycle - minCycle;
            string regularity;
            
            if (variation <= 3)
                regularity = "Очень регулярный";
            else if (variation <= 7)
                regularity = "Регулярный";
            else if (variation <= 14)
                regularity = "Умеренно регулярный";
            else
                regularity = "Нерегулярный";
                
            return new
            {
                Regularity = regularity,
                Variation = variation,
                AverageCycle = averageCycle
            };
        }

        private DateTime PredictNextPeriod(List<Period> periods, User user)
        {
            if (periods.Count == 0)
            {
                // Если нет истории, используем текущую дату и стандартный цикл пользователя
                _logger.LogInformation($"Нет истории менструаций для пользователя {user.UserId}, используем стандартный цикл");
                return DateTime.UtcNow.AddDays(user.CycleLength);
            }
            
            var lastPeriod = periods.OrderByDescending(p => p.StartDate).First();
            return lastPeriod.StartDate.AddDays(user.CycleLength);
        }

        private DateTime PredictNextOvulation(List<Period> periods, User user)
        {
            var nextPeriod = PredictNextPeriod(periods, user);
            // Овуляция обычно происходит за 14 дней до начала следующей менструации
            return nextPeriod.AddDays(-14);
        }
    }
}
