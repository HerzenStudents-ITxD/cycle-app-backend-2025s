using CycleApp.Models;

namespace CycleApp.Services
{
    public interface ICycleCalculatorService
    {
        (DateTime start, DateTime end) CalculateNextOvulation(User user);
        (DateTime start, DateTime end) CalculateNextPeriod(User user);
        int CalculateDayOfCycle(User user, DateTime date);
    }

    public class CycleCalculatorService : ICycleCalculatorService
    {
        public (DateTime start, DateTime end) CalculateNextOvulation(User user)
        {
            var lastPeriod = user.Periods?
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            DateTime ovulationStart;

            if (lastPeriod == null)
            {
                ovulationStart = DateTime.UtcNow.AddDays(user.CycleLength - 14);
            }
            else
            {
                ovulationStart = lastPeriod.StartDate.AddDays(user.CycleLength - 14);
            }

            return (ovulationStart, ovulationStart.AddDays(1));
        }

        // ВОССТАНОВЛЕННЫЙ МЕТОД ДЛЯ ПЕРИОДОВ
        public (DateTime start, DateTime end) CalculateNextPeriod(User user)
        {
            var lastPeriod = user.Periods?
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            DateTime periodStart;

            if (lastPeriod == null)
            {
                periodStart = DateTime.UtcNow.AddDays(user.CycleLength);
            }
            else
            {
                periodStart = lastPeriod.StartDate.AddDays(user.CycleLength);
            }

            return (
                start: periodStart,
                end: periodStart.AddDays(user.PeriodLength)
            );
        }

        public int CalculateDayOfCycle(User user, DateTime date)
        {
            var lastPeriod = user.Periods?
                .Where(p => p.StartDate <= date)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            if (lastPeriod == null) return 1;

            return (date - lastPeriod.StartDate).Days % user.CycleLength + 1;
        }
    }
}