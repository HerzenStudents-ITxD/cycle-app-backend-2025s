namespace CycleApp.Contracts.Auth
{
    public record VerifyCodeRequest(
        string Email,
        string Code
    );
}