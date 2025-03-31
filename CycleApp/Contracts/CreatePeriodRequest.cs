namespace CycleApp.Contracts;

public record CreatePeriodRequest(Guid user_id, DateTime StartDate, DateTime? EndDate, bool IsActive);