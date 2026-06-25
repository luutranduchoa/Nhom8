using System;
using System.Collections.Generic;

namespace Medicine.DTOs
{
    public class PrescriptionDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? Notes { get; set; }
        public List<PrescriptionDetailDto> Details { get; set; } = new List<PrescriptionDetailDto>();
        public List<InteractionWarningDto> Warnings { get; set; } = new List<InteractionWarningDto>();
    }

    public class PrescriptionDetailDto
    {
        public int DrugId { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string DosageInstruction { get; set; } = string.Empty;
    }

    public class CreatePrescriptionDto
    {
        public string PatientName { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<CreatePrescriptionDetailDto> Details { get; set; } = new List<CreatePrescriptionDetailDto>();
    }

    public class CreatePrescriptionDetailDto
    {
        public int DrugId { get; set; }
        public int Quantity { get; set; }
        public string DosageInstruction { get; set; } = string.Empty;
    }
    
    public class InteractionWarningDto
    {
        public string SourceDrugName { get; set; } = string.Empty;
        public string TargetDrugName { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSevere { get; set; } // Flag cảnh báo đỏ
    }
}
