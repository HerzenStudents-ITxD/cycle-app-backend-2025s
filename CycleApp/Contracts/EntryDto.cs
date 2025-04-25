namespace CycleApp.Contracts;

public record EntryDto(
    Guid EntryId,
    Guid UserId,
    DateTime Date,
    bool PeriodStarted,
    bool PeriodEnded,
    string? Note=null,
    string? Heaviness = null,
    string? Symptoms = null,
    string? Sex = null,
    string? Mood=null,
    string? Discharges = null);