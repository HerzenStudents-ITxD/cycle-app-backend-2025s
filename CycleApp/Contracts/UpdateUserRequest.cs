namespace CycleApp.Contracts
{
    public class UpdateUserRequest
    {
        public int? cycleLength { get; set; }
        public int? periodLength { get; set; }
        public bool? remindPeriod { get; set; }
        public bool? remindOvulation { get; set; }
    }
}