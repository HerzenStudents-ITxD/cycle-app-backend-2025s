//namespace CycleApp.Contracts;

//public record GetEntriesResponse(List<EntryDto> Entries);
//public record EntryDto(
//    Guid EntryId,
//    Guid user_id,
//    DateTime Date,
//    bool PeriodStarted,
//    bool PeriodEnded,
//    string? Note,
//    string? Heaviness,
//    string? Symptoms,
//    string? Sex,
//    string? Mood,
//    string? Discharges
//);
using System.Collections.Generic;

namespace CycleApp.Contracts
{
    public class GetEntriesResponse
    {
        public List<EntryDto> Entries { get; set; }

        public GetEntriesResponse(List<EntryDto> entries)
        {
            Entries = entries;
        }
    }

    public class EntryDto
    {
        public Guid entry_id { get; set; }
        public Guid user_id { get; set; }
        public DateTime Date { get; set; }
        public bool PeriodStarted { get; set; }
        public bool PeriodEnded { get; set; }
        public string Note { get; set; }
        public string Heaviness { get; set; }
        public string Symptoms { get; set; }
        public string Sex { get; set; }
        public string Mood { get; set; }
        public string Discharges { get; set; }

        public EntryDto(Guid entry_id, Guid user_id, DateTime date, bool periodStarted, bool periodEnded,
                       string note, string heaviness, string symptoms, string sex, string mood, string discharges)
        {
            this.entry_id = entry_id;
            this.user_id = user_id;
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
}