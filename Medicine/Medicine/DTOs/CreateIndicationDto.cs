namespace Medicine.DTOs
{
    public class CreateIndicationDto
    {
        public string Icd10Code { get; set; } = string.Empty;
        public string DiseaseName { get; set; } = string.Empty;
        public string DosageInstruction { get; set; } = string.Empty;
    }
}
