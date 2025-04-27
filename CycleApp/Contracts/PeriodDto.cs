namespace CycleApp.Contracts;

public record PeriodDto(
    Guid PeriodId,
    Guid UserId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool IsPredicted);