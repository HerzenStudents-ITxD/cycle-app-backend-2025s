namespace CycleApp.Models;

public class Period
{
    public Period(Guid user_id, DateTime startDate, DateTime? endDate, bool isActive)
    {
        user_id = user_id;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
    }

    public Guid period_id { get; set; }
    public Guid user_id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }

    public User User { get; set; }
}