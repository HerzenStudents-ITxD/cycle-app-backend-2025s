//namespace CycleApp.Models;

//public class Entry
//{
//    public Entry(
//        Guid user_id,
//        DateTime date,
//        bool periodStarted,
//        bool periodEnded,
//        string? note,
//        string? heaviness,
//        string? symptoms,
//        string? sex,
//        string? mood,
//        string? discharges)
//    {
//        user_id = user_id;
//        Date = date;
//        PeriodStarted = periodStarted;
//        PeriodEnded = periodEnded;
//        Note = note;
//        Heaviness = heaviness;
//        Symptoms = symptoms;
//        Sex = sex;
//        Mood = mood;
//        Discharges = discharges;
//    }

//    public Guid entry_id { get; set; }
//    public Guid user_id { get; set; }
//    public DateTime Date { get; set; }
//    public bool PeriodStarted { get; set; }
//    public bool PeriodEnded { get; set; }
//    public string? Note { get; set; }
//    public string? Heaviness { get; set; }
//    public string? Symptoms { get; set; }
//    public string? Sex { get; set; }
//    public string? Mood { get; set; }
//    public string? Discharges { get; set; }

//    public User User { get; set; }
//}
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CycleApp.Models
{
    public class Entry
    {
        [Key]
        public int EntryId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public virtual User? User { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public bool? PeriodStarted { get; set; }
        public bool? PeriodEnded { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        [StringLength(20)]
        public string? Heaviness { get; set; }

        // List of symptoms for this entry
        public List<EntrySymptom> Symptoms { get; set; } = new List<EntrySymptom>();

        [StringLength(20)]
        public string? Sex { get; set; }

        [StringLength(20)]
        public string? Mood { get; set; }

        [StringLength(20)]
        public string? Discharges { get; set; }

        // Reference to the period this entry belongs to
        public Guid? PeriodId { get; set; }
        
        [JsonIgnore]
        public virtual Period? Period { get; set; }
    }

    public class EntrySymptom
    {
        [Key]
        public int EntrySymptomId { get; set; }
        
        [Required]
        public int EntryId { get; set; }
        
        [JsonIgnore]
        public virtual Entry? Entry { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(20)]
        public string? Intensity { get; set; } // e.g., "Mild", "Moderate", "Severe"

        [StringLength(200)]
        public string? Notes { get; set; }
    }
}