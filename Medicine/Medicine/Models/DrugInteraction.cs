namespace Medicine.Models
{
    public class DrugInteraction
    {
        public int SourceDrugId { get; set; }
        public Drug SourceDrug { get; set; } = null!;

        public int TargetDrugId { get; set; }
        public Drug TargetDrug { get; set; } = null!;

        public string Severity { get; set; } = string.Empty; // Nặng, Trung bình, Nhẹ
        public string Description { get; set; } = string.Empty; // Mô tả tương tác
    }
}
