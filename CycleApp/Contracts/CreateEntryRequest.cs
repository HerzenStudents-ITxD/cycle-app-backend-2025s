using System;
using System.ComponentModel.DataAnnotations;

namespace CycleApp.Contracts
{
    public class CreateEntryRequest
    {
        [Required]
        public Guid user_id { get; set; }

        public DateTime? date { get; set; }

        public bool? periodStarted { get; set; }
        public bool? periodEnded { get; set; }

        [StringLength(500)]
        public string note { get; set; }

        [StringLength(20)]
        public string heaviness { get; set; }

        [StringLength(200)]
        public string symptoms { get; set; }

        [StringLength(20)]
        public string sex { get; set; }

        [StringLength(20)]
        public string mood { get; set; }

        [StringLength(20)]
        public string discharges { get; set; }
    }
}