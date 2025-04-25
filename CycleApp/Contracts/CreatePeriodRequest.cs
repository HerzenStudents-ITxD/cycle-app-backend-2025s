namespace CycleApp.Contracts
{
    public class CreatePeriodRequest
    {
        public Guid UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? DayBeforePeriod { get; set; }
    }
}