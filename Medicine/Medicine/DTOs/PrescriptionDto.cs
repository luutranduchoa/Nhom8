using System;
using System.Collections.Generic;

namespace Medicine.DTOs
{
    public class PrescriptionDto
    {
        public int Id { get; set; }
        public string PrescriptionCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public string PatientAge { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? Notes { get; set; }
        public List<PrescriptionDetailDto> PrescriptionDetails { get; set; } = new();
        public List<InteractionWarningDto> Warnings { get; set; } = new();
    }

    public class PrescriptionDetailDto
    {
        public int Id { get; set; }
        public int DrugId { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Instructions { get; set; } = string.Empty;
    }

    public class CreatePrescriptionDto
    {
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public string PatientAge { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<CreatePrescriptionDetailDto> Details { get; set; } = new();
    }

    public class CreatePrescriptionDetailDto
    {
        public int DrugId { get; set; }
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Instructions { get; set; } = string.Empty;
    }

    public class InteractionWarningDto
    {
        public string SourceDrugName { get; set; } = string.Empty;
        public string TargetDrugName { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSevere { get; set; }
    }
}
