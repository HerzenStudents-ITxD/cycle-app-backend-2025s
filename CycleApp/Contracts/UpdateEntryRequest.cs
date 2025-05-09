using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CycleApp.Contracts
{
    public class UpdateEntryRequest
    {
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
}