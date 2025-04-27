namespace CycleApp.Contracts;
public record OvulationDto(
    Guid OvulationId,
    Guid UserId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsPredicted,
    string Symptoms);