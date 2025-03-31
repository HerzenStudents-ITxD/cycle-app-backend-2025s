namespace CycleApp.Contracts;

public record CreateEntryRequest(
    Guid user_id,
    DateTime Date,
    bool PeriodStarted,
    bool PeriodEnded,
    string? Note,
    string? Heaviness,
    string? Symptoms,
    string? Sex,
    string? Mood,
    string? Discharges
);