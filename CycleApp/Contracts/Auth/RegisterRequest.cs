namespace CycleApp.Contracts.Auth


{
    using RegisterRequest = CycleApp.Contracts.Auth.RegisterRequest;
    public record RegisterRequest(
        string Email,
        string TempToken,
        int CycleLength,
        int PeriodLength
    );
}