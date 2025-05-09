using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CycleApp.Contracts
{
    public class CreateEntryRequest
    {
        [Required]
        public Guid user_id { get; set; }

        public DateTime? Date { get; set; }

        public bool PeriodStarted { get; set; }
        public bool PeriodEnded { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        [StringLength(20)]
        public string? Heaviness { get; set; }

        public List<SymptomRequest>? Symptoms { get; set; }

        [StringLength(20)]
        public string? Sex { get; set; }

        [StringLength(50)]
        public string? Mood { get; set; }

        [StringLength(100)]
        public string? Discharges { get; set; }
    }

    public class SymptomRequest
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        [StringLength(20)]
        public string? Intensity { get; set; }
        
        [StringLength(200)]
        public string? Notes { get; set; }
    }
}