using System.ComponentModel.DataAnnotations;

namespace CycleApp.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Range(21, 45, ErrorMessage = "Cycle length must be between 21 and 45 days")]
        public int CycleLength { get; set; }
        
        [Range(2, 10, ErrorMessage = "Period length must be between 2 and 10 days")]
        public int PeriodLength { get; set; }
        
        public bool RemindPeriod { get; set; }
        public bool RemindOvulation { get; set; }
        
        // Track cycle variations
        public int? MinCycleLength { get; set; }
        public int? MaxCycleLength { get; set; }
        public int? MinPeriodLength { get; set; }
        public int? MaxPeriodLength { get; set; }
        public DateTime? LastCycleVariationUpdate { get; set; }
        
        // Timezone information
        public string? TimeZoneId { get; set; }
        
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
