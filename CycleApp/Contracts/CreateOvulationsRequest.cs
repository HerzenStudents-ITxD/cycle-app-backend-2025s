namespace CycleApp.Contracts;

public record CreateOvulationRequest(
        Guid user_id,
        DateTime start_date,
        DateTime end_date,
        DateTime? predicted_start = null,
        int? days_until_ovulation = null);
