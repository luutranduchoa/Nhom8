using System;

namespace Medicine.Models
{
    public class InventoryTransaction
    {
        public int Id { get; set; }
        public int DrugId { get; set; }
        public Drug? Drug { get; set; }
        public int Quantity { get; set; } // Số lượng (+ là nhập, - là xuất)
        public string TransactionType { get; set; } = string.Empty; // "Import" (Nhập kho) hoặc "Export" (Xuất kho)
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }

        // Các trường mới được thêm vào từ UI quản lý tồn kho
        public string? Sku { get; set; } // Mã SKU
        public string? BatchNumber { get; set; } // Số lô
        public string? Unit { get; set; } // Đơn vị
        public decimal? ImportPrice { get; set; } // Giá nhập
        public string? Supplier { get; set; } // Nhà cung cấp
        public string? StorageLocation { get; set; } // Vị trí lưu kho
        public DateTime? ExpirationDate { get; set; } // Hạn dùng
    }
}
