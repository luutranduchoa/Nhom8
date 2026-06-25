using System.Collections.Generic;

namespace Medicine.DTOs
{
    public class SuggestedPrescriptionDto
    {
        public int DiseaseId { get; set; }
        public string DiseaseName { get; set; } = string.Empty;
        public List<SuggestedDrugDto> SuggestedDrugs { get; set; } = new List<SuggestedDrugDto>();
    }

    public class SuggestedDrugDto
    {
        public int DrugId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string DosageInstruction { get; set; } = string.Empty;
    }
}
