//namespace CycleApp.Models;

//public class Period
//{
//    public Period(Guid user_id, DateTime startDate, DateTime? endDate, bool isActive)
//    {
//        user_id = user_id;
//        StartDate = startDate;
//        EndDate = endDate;
//        IsActive = isActive;
//    }

//    public Guid period_id { get; set; }
//    public Guid user_id { get; set; }
//    public DateTime StartDate { get; set; }
//    public DateTime? EndDate { get; set; }
//    public bool IsActive { get; set; }

//    public User User { get; set; }
//}
namespace CycleApp.Models
{
    public class Period
    {
        public Guid PeriodId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? DayBeforePeriod { get; set; }
        public int DayOfCycle { get; set; }

        public User User { get; set; }

        public Period() { }

        public Period(Guid userId, DateTime startDate, DateTime? endDate, bool isActive, int? dayBeforePeriod = null)
        {
            PeriodId = Guid.NewGuid();
            UserId = userId;
            StartDate = startDate;
            EndDate = endDate;
            IsActive = isActive;
            DayBeforePeriod = dayBeforePeriod;
        }
    }
}