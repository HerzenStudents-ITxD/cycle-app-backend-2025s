namespace CycleApp.Contracts;
public record UpdateOvulationRequest(
    DateTime? start_date = null,
    DateTime? end_date = null,
    DateTime? predicted_start = null,
    int? days_until_ovulation = null);