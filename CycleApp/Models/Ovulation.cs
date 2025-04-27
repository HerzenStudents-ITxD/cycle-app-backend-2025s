//namespace CycleApp.Models;

//public class Ovulation
//{
//    public Ovulation(Guid user_id, DateTime startDate, DateTime endDate)
//    {
//        user_id = user_id;
//        StartDate = startDate;
//        EndDate = endDate;
//    }

//    public Guid ovulation_id { get; set; }
//    public Guid user_id { get; set; }
//    public DateTime StartDate { get; set; }
//    public DateTime EndDate { get; set; }

//    public User User { get; set; }
//}
namespace CycleApp.Models
{
    public class Ovulation
    {
        public Guid OvulationId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? DayBeforeOvulation { get; set; }
        public bool IsPredicted { get; set; } = false;

        public string? Symptoms { get; set; } = null;

        public User User { get; set; }
    }
}