namespace Medicine.Models
{
    public class Indication
    {
        public int DrugId { get; set; }
        public Drug Drug { get; set; } = null!;

        public int DiseaseId { get; set; }
        public Disease Disease { get; set; } = null!;

        public string? DosageInstruction { get; set; } // Liều dùng cụ thể cho bệnh này
    }
}
