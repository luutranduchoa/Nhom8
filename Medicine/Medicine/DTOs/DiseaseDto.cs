namespace Medicine.DTOs
{
    public class DiseaseDto
    {
        public string Icd10Code { get; set; } = string.Empty;
        public string DiseaseName { get; set; } = string.Empty;
        public string? Notes { get; set; } // Chứa liều dùng hoặc cảnh báo
    }
}

