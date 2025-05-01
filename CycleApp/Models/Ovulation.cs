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

        public User? User { get; set; }
    }
}