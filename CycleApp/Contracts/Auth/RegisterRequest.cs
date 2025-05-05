namespace CycleApp.Contracts.Auth
{
    public record RegisterRequest(
        string Email,
        int CycleLength,
        int PeriodLength
    );
}