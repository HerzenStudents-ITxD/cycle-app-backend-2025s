using System;
using System.Threading.Tasks;

namespace CycleApp.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendPeriodReminderAsync(int userId, DateTime expectedDate);
        Task SendOvulationReminderAsync(int userId, DateTime expectedDate);
        Task<bool> ScheduleNotifications(int userId);
        Task<bool> CancelNotifications(int userId);
    }
}
