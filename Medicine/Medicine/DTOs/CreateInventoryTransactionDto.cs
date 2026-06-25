namespace Medicine.DTOs
{
    public class CreateInventoryTransactionDto
    {
        public int DrugId { get; set; }
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "Import" (Nhập) hoặc "Export" (Xuất)
        public string? Note { get; set; }

        // Các trường bổ sung cho UI
        public string? Sku { get; set; }
        public string? BatchNumber { get; set; }
        public int? MinStockLevel { get; set; } // Nếu có, cập nhật lại cho Drug
        public string? Unit { get; set; }
        public decimal? ImportPrice { get; set; }
        public string? Supplier { get; set; }
        public string? StorageLocation { get; set; }
        public System.DateTime? ExpirationDate { get; set; }
    }
}
