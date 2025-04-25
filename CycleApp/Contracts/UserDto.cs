namespace CycleApp.Contracts;

public record UserDto(
    Guid UserId,
    string Email,
    int CycleLength,
    int PeriodLength,
    bool RemindPeriod,
    bool RemindOvulation);