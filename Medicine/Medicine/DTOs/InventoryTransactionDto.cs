using System;

namespace Medicine.DTOs
{
    public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public int DrugId { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string? Note { get; set; }

        public string? Sku { get; set; }
        public string? BatchNumber { get; set; }
        public string? Unit { get; set; }
        public decimal? ImportPrice { get; set; }
        public string? Supplier { get; set; }
        public string? StorageLocation { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? MinStockLevel { get; set; } // Phục vụ hiển thị trên UI
    }
}
