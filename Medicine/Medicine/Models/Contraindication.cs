namespace Medicine.Models
{
    public class Contraindication
    {
        public int DrugId { get; set; }
        public Drug Drug { get; set; } = null!;

        public int DiseaseId { get; set; }
        public Disease Disease { get; set; } = null!;

        public string? WarningNotes { get; set; } // Mức độ nguy hiểm / Lưu ý
    }
}
