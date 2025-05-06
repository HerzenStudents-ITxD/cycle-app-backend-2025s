using CycleApp.Models;
using System.Threading.Tasks;

namespace CycleApp.Services.Interfaces
{
    public interface IUserSettingsService
    {
        Task<User> GetUserSettings(int userId);
        Task<User> UpdateUserSettings(int userId, User settings);
        Task<bool> ToggleNotificationSettings(int userId, string notificationType, bool enabled);
        Task<bool> UpdateTheme(int userId, string theme);
    }
}
