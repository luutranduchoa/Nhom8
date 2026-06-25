namespace Medicine.Models
{
    public class Disease
    {
        public int Id { get; set; }
        public string Icd10Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public ICollection<Indication> Indications { get; set; } = new List<Indication>();
        public ICollection<Contraindication> Contraindications { get; set; } = new List<Contraindication>();

    }
}
