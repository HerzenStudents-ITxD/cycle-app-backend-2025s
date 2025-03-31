namespace CycleApp.Models;

public class User
{
    public User(string email, int cycleLength, DateTime createDate, int remindPeriod, bool remindOvulation, int periodLength, string theme)
    {
        Email = email;
        CycleLength = cycleLength;
        CreateDate = createDate;
        RemindPeriod = remindPeriod;
        RemindOvulation = remindOvulation;
        PeriodLength = periodLength;
        Theme = theme;
    }

    public Guid user_id { get; set; }
    public string Email { get; set; }
    public int CycleLength { get; set; }
    public DateTime CreateDate { get; set; }
    public int RemindPeriod { get; set; }
    public bool RemindOvulation { get; set; }
    public int PeriodLength { get; set; }
    public string Theme { get; set; }

    public ICollection<Period> Periods { get; set; }
    public ICollection<Ovulation> Ovulations { get; set; }
    public ICollection<Entry> Entries { get; set; }
}