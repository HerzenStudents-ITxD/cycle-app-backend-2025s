using CycleApp.Models;
using System.Globalization;

namespace CycleApp.Services
{
    public interface ICycleCalculatorService
    {
        (DateTime start, DateTime end) CalculateNextOvulation(User user, DateTime? baseDate = null);
        (DateTime start, DateTime end) CalculateNextPeriod(User user, DateTime? baseDate = null);
        int CalculateDayOfCycle(User user, DateTime date);
        void UpdateCycleVariations(User user);
    }

    public class CycleCalculatorService : ICycleCalculatorService
    {
        private const int MIN_CYCLES_FOR_VARIATION = 3;
        private const int LUTEAL_PHASE_DAYS = 14;
        private const int MAX_CYCLE_VARIATION_DAYS = 5;

        public (DateTime start, DateTime end) CalculateNextOvulation(User user, DateTime? baseDate = null)
        {
            var lastPeriod = user.Periods?
                .Where(p => !p.IsPredicted)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            DateTime ovulationStart;
            int cycleLength = GetAdjustedCycleLength(user);

            if (lastPeriod == null)
            {
                ovulationStart = (baseDate ?? DateTime.UtcNow).AddDays(cycleLength - LUTEAL_PHASE_DAYS);
            }
            else
            {
                ovulationStart = (baseDate ?? lastPeriod.StartDate).AddDays(cycleLength - LUTEAL_PHASE_DAYS);
            }

            // Adjust for timezone if specified
            if (!string.IsNullOrEmpty(user.TimeZoneId))
            {
                try
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
                    ovulationStart = TimeZoneInfo.ConvertTimeFromUtc(ovulationStart, timeZone);
                }
                catch (TimeZoneNotFoundException)
                {
                    // If timezone is invalid, use UTC
                }
            }

            return (ovulationStart, ovulationStart.AddDays(1));
        }

        public (DateTime start, DateTime end) CalculateNextPeriod(User user, DateTime? baseDate = null)
        {
            var lastPeriod = user.Periods?
                .Where(p => !p.IsPredicted)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            DateTime periodStart;
            int cycleLength = GetAdjustedCycleLength(user);
            int periodLength = GetAdjustedPeriodLength(user);

            if (lastPeriod == null)
            {
                periodStart = (baseDate ?? DateTime.UtcNow).AddDays(cycleLength);
            }
            else
            {
                periodStart = (baseDate ?? lastPeriod.StartDate).AddDays(cycleLength);
            }

            // Adjust for timezone if specified
            if (!string.IsNullOrEmpty(user.TimeZoneId))
            {
                try
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
                    periodStart = TimeZoneInfo.ConvertTimeFromUtc(periodStart, timeZone);
                }
                catch (TimeZoneNotFoundException)
                {
                    // If timezone is invalid, use UTC
                }
            }

            return (
                start: periodStart,
                end: periodStart.AddDays(periodLength)
            );
        }

        public int CalculateDayOfCycle(User user, DateTime date)
        {
            var lastPeriod = user.Periods?
                .Where(p => !p.IsPredicted && p.StartDate <= date)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            if (lastPeriod == null) return 1;

            return (date - lastPeriod.StartDate).Days % GetAdjustedCycleLength(user) + 1;
        }

        public void UpdateCycleVariations(User user)
        {
            var actualPeriods = user.Periods
                .Where(p => !p.IsPredicted)
                .OrderBy(p => p.StartDate)
                .ToList();

            if (actualPeriods.Count < MIN_CYCLES_FOR_VARIATION)
                return;

            var cycleLengths = new List<int>();
            var periodLengths = new List<int>();

            for (int i = 1; i < actualPeriods.Count; i++)
            {
                var cycleLength = (actualPeriods[i].StartDate - actualPeriods[i - 1].StartDate).Days;
                if (cycleLength > 0 && cycleLength <= user.CycleLength + MAX_CYCLE_VARIATION_DAYS)
                {
                    cycleLengths.Add(cycleLength);
                }

                if (actualPeriods[i - 1].EndDate.HasValue)
                {
                    var periodLength = (actualPeriods[i - 1].EndDate.Value - actualPeriods[i - 1].StartDate).Days;
                    if (periodLength > 0 && periodLength <= user.PeriodLength + MAX_CYCLE_VARIATION_DAYS)
                    {
                        periodLengths.Add(periodLength);
                    }
                }
            }

            if (cycleLengths.Any())
            {
                user.MinCycleLength = cycleLengths.Min();
                user.MaxCycleLength = cycleLengths.Max();
            }

            if (periodLengths.Any())
            {
                user.MinPeriodLength = periodLengths.Min();
                user.MaxPeriodLength = periodLengths.Max();
            }

            user.LastCycleVariationUpdate = DateTime.UtcNow;
        }

        private int GetAdjustedCycleLength(User user)
        {
            if (user.MinCycleLength.HasValue && user.MaxCycleLength.HasValue)
            {
                // Use average of min and max if available
                return (user.MinCycleLength.Value + user.MaxCycleLength.Value) / 2;
            }
            return user.CycleLength;
        }

        private int GetAdjustedPeriodLength(User user)
        {
            if (user.MinPeriodLength.HasValue && user.MaxPeriodLength.HasValue)
            {
                // Use average of min and max if available
                return (user.MinPeriodLength.Value + user.MaxPeriodLength.Value) / 2;
            }
            return user.PeriodLength;
        }
    }
}