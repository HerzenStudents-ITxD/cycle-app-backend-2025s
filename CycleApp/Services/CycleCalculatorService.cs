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
            // Ovulation typically occurs 14 days before the next period
            var lastPeriod = user.Periods?.OrderByDescending(p => p.StartDate).FirstOrDefault();

            if (lastPeriod == null)
            {
                // Default calculation if no period data
                var nextPeriodStart = DateTime.UtcNow.AddDays(user.CycleLength);
                var ovulationStart = nextPeriodStart.AddDays(-14);
                return (ovulationStart, ovulationStart.AddDays(5)); // Typically 5-6 days window
            }

            var cycleDay = CalculateDayOfCycle(user, DateTime.UtcNow);
            var daysUntilOvulation = lastPeriod.DayOfCycle - 14;

            if (daysUntilOvulation > 0)
            {
                var ovulationStart = DateTime.UtcNow.AddDays(daysUntilOvulation);
                return (ovulationStart, ovulationStart.AddDays(5));
            }
            else
            {
                var nextPeriodStart = lastPeriod.StartDate.AddDays(user.CycleLength);
                var ovulationStart = nextPeriodStart.AddDays(-14);
                return (ovulationStart, ovulationStart.AddDays(5));
            }
        }

        public (DateTime start, DateTime end) CalculateNextPeriod(User user)
        {
            var lastPeriod = user.Periods?.OrderByDescending(p => p.StartDate).FirstOrDefault();
            var start = lastPeriod == null
                ? DateTime.UtcNow.AddDays(user.CycleLength)
                : lastPeriod.StartDate.AddDays(user.CycleLength);

            return (start, start.AddDays(user.PeriodLength));
        }

        public int CalculateDayOfCycle(User user, DateTime date)
        {
            var lastPeriod = user.Periods?
                .Where(p => p.StartDate <= date)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            if (lastPeriod == null) return 1;

            var days = (date - lastPeriod.StartDate).Days;
            return (days % user.CycleLength) + 1;
        }
    }
}