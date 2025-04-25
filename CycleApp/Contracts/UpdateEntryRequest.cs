using System;

namespace CycleApp.Contracts
{
    public class UpdateEntryRequest
    {
        public DateTime? date { get; set; }
        public bool? periodStarted { get; set; }
        public bool? periodEnded { get; set; }
        public string note { get; set; }
        public string heaviness { get; set; }
        public string symptoms { get; set; }
        public string sex { get; set; }
        public string mood { get; set; }
        public string discharges { get; set; }
    }
}