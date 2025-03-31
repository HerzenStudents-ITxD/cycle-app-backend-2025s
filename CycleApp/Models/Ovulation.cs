namespace CycleApp.Models;

public class Ovulation
{
    public Ovulation(Guid user_id, DateTime startDate, DateTime endDate)
    {
        user_id = user_id;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Guid ovulation_id { get; set; }
    public Guid user_id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public User User { get; set; }
}