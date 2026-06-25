namespace Medicine.DTOs
{
    public class CreateContraindicationDto
    {
        public string Icd10Code { get; set; } = string.Empty;
        public string DiseaseName { get; set; } = string.Empty;
        public string WarningNotes { get; set; } = string.Empty; // Ghi chú cảnh báo
    }
}

