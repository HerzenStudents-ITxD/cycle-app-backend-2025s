using System;
using System.Collections.Generic;

namespace CycleApp.Contracts
{
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
} 