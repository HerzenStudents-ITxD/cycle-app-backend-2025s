
namespace CycleApp.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public int CycleLength { get; set; }
        public int PeriodLength { get; set; }
        public bool RemindPeriod { get; set; }
        public bool RemindOvulation { get; set; }

        public List<Period> Periods { get; set; }
        public List<Ovulation> Ovulations { get; set; }
        public List<Entry> Entries { get; set; }


        public User()
        {
            Periods = new List<Period>();
            Ovulations = new List<Ovulation>();
            Entries = new List<Entry>();
        }
    }
}
