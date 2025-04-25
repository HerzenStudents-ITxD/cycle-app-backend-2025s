namespace CycleApp.Contracts;
public record OvulationDto(
    Guid ovulation_id,
    Guid user_id,
    DateTime start_date,
    DateTime end_date,
    DateTime? predicted_start,
    int? days_until_ovulation);