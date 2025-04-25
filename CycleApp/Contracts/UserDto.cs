namespace CycleApp.Contracts
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public int CycleLength { get; set; }
        public int PeriodLength { get; set; }
        public bool RemindPeriod { get; set; }
        public bool RemindOvulation { get; set; }

        public UserDto(Guid userId, string email, int cycleLength, int periodLength, bool remindPeriod, bool remindOvulation)
        {
            UserId = userId;
            Email = email;
            CycleLength = cycleLength;
            PeriodLength = periodLength;
            RemindPeriod = remindPeriod;
            RemindOvulation = remindOvulation;
        }
    }
}