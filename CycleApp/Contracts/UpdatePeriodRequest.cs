using System;

namespace CycleApp.Contracts
{
    public class UpdatePeriodRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPredicted { get; set; }
        public int DayOfCycle { get; set; }
    }
} 