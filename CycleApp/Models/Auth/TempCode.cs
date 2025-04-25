namespace CycleApp.Models.Auth
{
    public class TempCode
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsUsed { get; set; }
    }
}