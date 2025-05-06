using System;
using System.Threading.Tasks;

namespace CycleApp.Services.Interfaces
{
    public interface ICycleAnalyticsService
    {
        Task<object> GetCycleAnalytics(int userId, int cyclesCount = 6);
        Task<object> GetAverageCycleDuration(int userId, int cyclesCount = 6);
        Task<object> GetAveragePeriodDuration(int userId, int cyclesCount = 6);
        Task<object> GetRegularityAnalysis(int userId, int cyclesCount = 6);
    }
}
