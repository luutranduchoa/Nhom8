namespace Medicine.DTOs
{
    public class CreateDrugDto
    {
        public string BrandName { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string DosageForm { get; set; } = string.Empty;
        public List<CreateIndicationDto> Indications { get; set; } = new();
        public List<CreateContraindicationDto> Contraindications { get; set; } = new(); // Thêm danh sách chống chỉ định
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
    }
}
