namespace CycleApp.Models;

public class Entry
{
    public Entry(
        Guid user_id,
        DateTime date,
        bool periodStarted,
        bool periodEnded,
        string? note,
        string? heaviness,
        string? symptoms,
        string? sex,
        string? mood,
        string? discharges)
    {
        user_id = user_id;
        Date = date;
        PeriodStarted = periodStarted;
        PeriodEnded = periodEnded;
        Note = note;
        Heaviness = heaviness;
        Symptoms = symptoms;
        Sex = sex;
        Mood = mood;
        Discharges = discharges;
    }

    public Guid entry_id { get; set; }
    public Guid user_id { get; set; }
    public DateTime Date { get; set; }
    public bool PeriodStarted { get; set; }
    public bool PeriodEnded { get; set; }
    public string? Note { get; set; }
    public string? Heaviness { get; set; }
    public string? Symptoms { get; set; }
    public string? Sex { get; set; }
    public string? Mood { get; set; }
    public string? Discharges { get; set; }

    public User User { get; set; }
}
