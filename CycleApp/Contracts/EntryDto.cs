using System;
using System.Collections.Generic;

namespace CycleApp.Contracts
{
    public class EntryDto
    {
        public int EntryId { get; set; }
        public Guid UserId { get; set; }
        public Guid? PeriodId { get; set; }
        public DateTime Date { get; set; }
        public bool PeriodStarted { get; set; }
        public bool PeriodEnded { get; set; }
        public string? Note { get; set; }
        public string? Heaviness { get; set; }
        public List<SymptomDto> Symptoms { get; set; }
        public string? Sex { get; set; }
        public string? Mood { get; set; }
        public string? Discharges { get; set; }

        public EntryDto(
            int entryId,
            Guid userId,
            Guid? periodId,
            DateTime date,
            bool periodStarted,
            bool periodEnded,
            string? note,
            string? heaviness,
            List<SymptomDto> symptoms,
            string? sex,
            string? mood,
            string? discharges)
        {
            EntryId = entryId;
            UserId = userId;
            PeriodId = periodId;
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
    }

    public class SymptomDto
    {
        public int EntrySymptomId { get; set; }
        public int EntryId { get; set; }
        public string Name { get; set; }
        public string? Intensity { get; set; }
        public string? Notes { get; set; }

        public SymptomDto(
            int entrySymptomId,
            int entryId,
            string name,
            string? intensity,
            string? notes)
        {
            EntrySymptomId = entrySymptomId;
            EntryId = entryId;
            Name = name;
            Intensity = intensity;
            Notes = notes;
        }
    }
}