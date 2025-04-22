//namespace CycleApp.Contracts.Auth
//{
//    public record AuthResponse(
//        string Token,
//        bool IsNewUser,
//        string Email,
//        Guid? UserId = null
//    );
//}
public class AuthResponse
{
    public string Token { get; set; }
    public bool IsNewUser { get; set; }
    public string Email { get; set; }
    public Guid? UserId { get; set; }

    public AuthResponse(string token, bool isNewUser, string email, Guid? userId = null)
    {
        Token = token;
        IsNewUser = isNewUser;
        Email = email;
        UserId = userId;
    }
}