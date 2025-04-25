namespace CycleApp.Contracts;

public record GetPeriodsResponse(List<PeriodDto> Periods);
public record PeriodDto(
    Guid period_id,
    Guid user_id,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive
);