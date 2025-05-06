using System.ComponentModel.DataAnnotations;

namespace CycleApp.Contracts.Auth
{
    public class CompleteRegistrationRequest
    {
        [Required]
        public string Token { get; set; }
        
        public int CycleLength { get; set; } = 28;
        
        public int PeriodLength { get; set; } = 5;
    }
}
