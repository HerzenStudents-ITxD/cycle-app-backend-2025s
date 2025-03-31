namespace CycleApp.Contracts;

public record UserDto(
    Guid user_id,
    string Email,
    int CycleLength,
    DateTime CreateDate,
    int RemindPeriod,
    bool RemindOvulation,
    int PeriodLength,
    string Theme
);