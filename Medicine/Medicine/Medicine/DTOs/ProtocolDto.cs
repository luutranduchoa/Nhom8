using System.Collections.Generic;

namespace Medicine.DTOs
{
    public class ProtocolDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? DiseaseId { get; set; }
        public string? DiseaseName { get; set; }
        public string? IcdCode { get; set; }
        public string? Notes { get; set; }
        public List<ProtocolDetailDto> Details { get; set; } = new List<ProtocolDetailDto>();
        public List<InteractionWarningDto> Warnings { get; set; } = new List<InteractionWarningDto>();
    }

    public class ProtocolDetailDto
    {
        public int DrugId { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string DosageInstruction { get; set; } = string.Empty;
    }

    public class CreateProtocolDto
    {
        public string Title { get; set; } = string.Empty;
        public int? DiseaseId { get; set; }
        public string? Notes { get; set; }
        public List<CreateProtocolDetailDto> Details { get; set; } = new List<CreateProtocolDetailDto>();
    }

    public class CreateProtocolDetailDto
    {
        public int DrugId { get; set; }
        public string DosageInstruction { get; set; } = string.Empty;
    }
}
