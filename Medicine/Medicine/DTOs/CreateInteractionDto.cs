using System.ComponentModel.DataAnnotations;

namespace Medicine.DTOs
{
    public class CreateInteractionDto
    {
        [Required]
        public int SourceDrugId { get; set; }
        
        [Required]
        public int TargetDrugId { get; set; }
        
        public string Severity { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
    }
}