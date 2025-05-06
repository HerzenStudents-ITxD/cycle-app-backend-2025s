using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CycleApp.Services
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly CycleDbContext _dbContext;
        private readonly ILogger<UserSettingsService> _logger;

        public UserSettingsService(CycleDbContext dbContext, ILogger<UserSettingsService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<User> GetUserSettings(int userId)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User> UpdateUserSettings(int userId, User settings)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found when updating settings", userId);
                return null;
            }
                
            user.CycleLength = settings.CycleLength;
            user.PeriodLength = settings.PeriodLength;
            user.Theme = settings.Theme;
            user.RemindPeriod = settings.RemindPeriod;
            user.RemindOvulation = settings.RemindOvulation;
            
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Updated settings for user {UserId}", userId);
            return user;
        }

        public async Task<bool> ToggleNotificationSettings(int userId, string notificationType, bool enabled)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found when changing notification settings", userId);
                return false;
            }
                
            switch (notificationType.ToLower())
            {
                case "period":
                    user.RemindPeriod = enabled;
                    break;
                case "ovulation":
                    user.RemindOvulation = enabled;
                    break;
                default:
                    _logger.LogWarning("Unknown notification type: {NotificationType}", notificationType);
                    return false;
            }
            
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Updated {NotificationType} notification settings for user {UserId}: {Enabled}", notificationType, userId, enabled);
            return true;
        }

        public async Task<bool> UpdateTheme(int userId, string theme)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found when changing theme", userId);
                return false;
            }
                
            user.Theme = theme;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Updated theme for user {UserId}: {Theme}", userId, theme);
            return true;
        }
    }
}
