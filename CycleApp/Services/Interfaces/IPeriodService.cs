using CycleApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CycleApp.Services.Interfaces
{
    public interface IPeriodService
    {
        Task<bool> ToggleMenstruationStatus(int userId, bool isStarted);
        Task<Period> StartPeriod(int userId, DateTime? startDate = null);
        Task<Period> EndPeriod(int userId, DateTime? endDate = null);
        Task<List<Period>> GetUserPeriods(int userId, int count = 6);
        Task<Period> GetActivePeriod(int userId);
    }
}
