using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CycleApp.DataAccess;
using CycleApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleApp.Services
{
    public interface IPeriodTableService
    {
        Task<PeriodTableDto> GetPeriodTableAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
    }

    public class PeriodTableService : IPeriodTableService
    {
        private readonly CycleDbContext _dbContext;

        public PeriodTableService(CycleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PeriodTableDto> GetPeriodTableAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbContext.Periods
                .Include(p => p.Entries)
                    .ThenInclude(e => e.Symptoms)
                .Where(p => p.UserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(p => 
                    p.StartDate >= startDate.Value || 
                    (p.EndDate.HasValue && p.EndDate.Value >= startDate.Value) ||
                    (!p.EndDate.HasValue && p.StartDate <= startDate.Value)
                );
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => 
                    p.StartDate <= endDate.Value ||
                    (p.EndDate.HasValue && p.EndDate.Value <= endDate.Value) ||
                    (!p.EndDate.HasValue && p.StartDate <= endDate.Value)
                );
            }

            var periods = await query
                .OrderBy(p => p.StartDate)
                .ToListAsync();

            if (!periods.Any())
                return new PeriodTableDto 
                { 
                    Periods = new List<PeriodTableRowDto>(),
                    Statistics = new PeriodStatisticsDto()
                };

            var rows = new List<PeriodTableRowDto>();
            var cycleLengths = new List<int>();
            var periodLengths = new List<int>();

            for (int i = 0; i < periods.Count; i++)
            {
                var period = periods[i];
                var previousPeriod = i > 0 ? periods[i - 1] : null;
                var cycleLength = previousPeriod != null ? (int)(period.StartDate - previousPeriod.StartDate).TotalDays : 0;
                var periodLength = period.EndDate.HasValue ? (int)(period.EndDate.Value - period.StartDate).TotalDays : 0;

                if (cycleLength > 0 && !period.IsPredicted)
                    cycleLengths.Add(cycleLength);
                if (periodLength > 0 && !period.IsPredicted)
                    periodLengths.Add(periodLength);

                // Get all entries for this period
                var entries = period.Entries.Select(e => new EntryDto(
                    e.EntryId,
                    e.UserId,
                    e.PeriodId ?? Guid.Empty,
                    e.Date,
                    e.PeriodStarted ?? false,
                    e.PeriodEnded ?? false,
                    e.Note,
                    e.Heaviness,
                    e.Symptoms.Select(s => new SymptomDto(
                        s.EntrySymptomId.GetHashCode(),
                        e.EntryId,
                        s.Name,
                        s.Intensity,
                        s.Notes
                    )).ToList(),
                    e.Sex,
                    e.Mood,
                    e.Discharges
                )).ToList();

                rows.Add(new PeriodTableRowDto
                {
                    PeriodId = period.PeriodId,
                    StartDate = period.StartDate,
                    EndDate = period.EndDate,
                    IsActive = period.IsActive,
                    IsPredicted = period.IsPredicted,
                    CycleLength = cycleLength,
                    PeriodLength = periodLength,
                    DayOfCycle = period.DayOfCycle,
                    Entries = entries
                });
            }

            return new PeriodTableDto
            {
                Periods = rows,
                Statistics = new PeriodStatisticsDto
                {
                    AverageCycleLength = cycleLengths.Any() ? cycleLengths.Average() : null,
                    MinCycleLength = cycleLengths.Any() ? cycleLengths.Min() : null,
                    MaxCycleLength = cycleLengths.Any() ? cycleLengths.Max() : null,
                    AveragePeriodLength = periodLengths.Any() ? periodLengths.Average() : null,
                    MinPeriodLength = periodLengths.Any() ? periodLengths.Min() : null,
                    MaxPeriodLength = periodLengths.Any() ? periodLengths.Max() : null,
                    TotalPeriods = periods.Count(p => !p.IsPredicted),
                    TotalPredictedPeriods = periods.Count(p => p.IsPredicted)
                }
            };
        }
    }

    public class PeriodTableDto
    {
        public List<PeriodTableRowDto> Periods { get; set; } = new List<PeriodTableRowDto>();
        public PeriodStatisticsDto Statistics { get; set; } = new PeriodStatisticsDto();
    }

    public class PeriodTableRowDto
    {
        public Guid PeriodId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int CycleLength { get; set; }
        public int PeriodLength { get; set; }
        public int DayOfCycle { get; set; }
        public bool IsActive { get; set; }
        public bool IsPredicted { get; set; }
        public List<EntryDto> Entries { get; set; } = new List<EntryDto>();
    }

    public class PeriodStatisticsDto
    {
        public double? AverageCycleLength { get; set; }
        public int? MinCycleLength { get; set; }
        public int? MaxCycleLength { get; set; }
        public double? AveragePeriodLength { get; set; }
        public int? MinPeriodLength { get; set; }
        public int? MaxPeriodLength { get; set; }
        public int TotalPeriods { get; set; }
        public int TotalPredictedPeriods { get; set; }
    }

    public class EntryDto
    {
        public int EntryId { get; set; }
        public Guid UserId { get; set; }
        public Guid PeriodId { get; set; }
        public DateTime Date { get; set; }
        public bool PeriodStarted { get; set; }
        public bool PeriodEnded { get; set; }
        public string? Note { get; set; }
        public string? Heaviness { get; set; }
        public string? Sex { get; set; }
        public string? Mood { get; set; }
        public string? Discharges { get; set; }
        public List<SymptomDto> Symptoms { get; set; }

        public EntryDto(
            int entryId,
            Guid userId,
            Guid periodId,
            DateTime date,
            bool periodStarted,
            bool periodEnded,
            string? note,
            string? heaviness,
            List<SymptomDto> symptoms,
            string? sex,
            string? mood,
            string? discharges)
        {
            EntryId = entryId;
            UserId = userId;
            PeriodId = periodId;
            Date = date;
            PeriodStarted = periodStarted;
            PeriodEnded = periodEnded;
            Note = note;
            Heaviness = heaviness;
            Sex = sex;
            Mood = mood;
            Discharges = discharges;
            Symptoms = symptoms;
        }
    }

    public class SymptomDto
    {
        public int EntrySymptomId { get; set; }
        public int EntryId { get; set; }
        public string Name { get; set; }
        public string? Intensity { get; set; }
        public string? Notes { get; set; }

        public SymptomDto(
            int entrySymptomId,
            int entryId,
            string name,
            string? intensity,
            string? notes)
        {
            EntrySymptomId = entrySymptomId;
            EntryId = entryId;
            Name = name;
            Intensity = intensity;
            Notes = notes;
        }
    }
} 