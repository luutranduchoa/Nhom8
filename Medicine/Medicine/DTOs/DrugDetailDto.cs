namespace Medicine.DTOs
{
    public class DrugDetailDto
    {
        public int Id { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string DosageForm { get; set; } = string.Empty;

        public List<DiseaseDto> Indications { get; set; } = new();
        public List<DiseaseDto> Contraindications { get; set; } = new();
        public List<InteractionDto> Interactions { get; set; } = new();
    }
}

