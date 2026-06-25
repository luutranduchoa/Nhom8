namespace Medicine.Models
{
    public class DrugCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Mối quan hệ 1-Nhiều: Một loại có nhiều thuốc
        public ICollection<Drug>? Drugs { get; set; }
    }
}
