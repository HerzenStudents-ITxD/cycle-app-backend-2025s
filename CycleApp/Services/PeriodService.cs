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
    public class PeriodService : IPeriodService
    {
        private readonly CycleDbContext _dbContext;
        private readonly ILogger<PeriodService> _logger;

        public PeriodService(CycleDbContext dbContext, ILogger<PeriodService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<bool> ToggleMenstruationStatus(int userId, bool isStarted)
        {
            var activePeriod = await GetActivePeriod(userId);
            
            if (isStarted && activePeriod == null)
            {
                // Start period if not active
                await StartPeriod(userId);
                _logger.LogInformation("Period started for user {UserId}", userId);
                return true;
            }
            else if (!isStarted && activePeriod != null)
            {
                // End period if active
                await EndPeriod(userId);
                _logger.LogInformation("Period ended for user {UserId}", userId);
                return true;
            }
            
            return false; // No changes
        }

        public async Task<Period> StartPeriod(int userId, DateTime? startDate = null)
        {
            var date = startDate ?? DateTime.UtcNow;
            
            // Make sure there's no active period
            var activePeriod = await GetActivePeriod(userId);
            if (activePeriod != null)
            {
                _logger.LogWarning("Attempt to start period when there's already an active one for user {UserId}", userId);
                return activePeriod; // Already has active period
            }
            
            // Create new period
            var period = new Period
            {
                UserId = userId,
                StartDate = date,
                IsActive = true
            };
            
            _dbContext.Periods.Add(period);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Created new period for user {UserId} with start date {Date}", userId, date);
            
            return period;
        }

        public async Task<Period> EndPeriod(int userId, DateTime? endDate = null)
        {
            var date = endDate ?? DateTime.UtcNow;
            
            // Find active period
            var period = await GetActivePeriod(userId);
            if (period == null)
            {
                _logger.LogWarning("Attempt to end period when there's no active one for user {UserId}", userId);
                return null; // No active period
            }
            
            // End period
            period.EndDate = date;
            period.IsActive = false;
            
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Ended period {PeriodId} for user {UserId} with end date {Date}", period.PeriodId, userId, date);
            
            return period;
        }

        public async Task<List<Period>> GetUserPeriods(int userId, int count = 6)
        {
            return await _dbContext.Periods
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.StartDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Period> GetActivePeriod(int userId)
        {
            return await _dbContext.Periods
                .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);
        }
    }
}
