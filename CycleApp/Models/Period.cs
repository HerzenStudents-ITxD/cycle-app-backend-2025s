using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CycleApp.Models
{
    public class Period
    {
        [Key]
        public Guid PeriodId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [Required]
        public bool IsActive { get; set; }
        
        [Required]
        public bool IsPredicted { get; set; }
        
        public int DayOfCycle { get; set; }
        
        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual User User { get; set; }
        
        [JsonIgnore]
        public List<Entry> Entries { get; set; } = new List<Entry>();

        public int? DayBeforePeriod { get; set; }

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