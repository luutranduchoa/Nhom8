namespace Medicine.DTOs
{
    public class UpdateInventoryTransactionDto
    {
        public decimal? ImportPrice { get; set; }
        public string? Note { get; set; }
        public string? Sku { get; set; }
        public string? BatchNumber { get; set; }
        public string? Unit { get; set; }
        public string? Supplier { get; set; }
        public string? StorageLocation { get; set; }
        public System.DateTime? ExpirationDate { get; set; }
    }
}
