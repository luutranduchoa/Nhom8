namespace Medicine.Models
{
    public class PrescriptionDetail
    {
        public int Id { get; set; }

        public int PrescriptionId { get; set; }
        public Prescription? Prescription { get; set; }

        public int DrugId { get; set; }
        public Drug? Drug { get; set; }

        public string Dosage { get; set; } = string.Empty; // Liều dùng (VD: 1 viên, 2ml)
        public string Frequency { get; set; } = string.Empty; // Tần suất (VD: 3 lần/ngày, sáng tối)
        public int Duration { get; set; } // Số ngày dùng
        public string Instructions { get; set; } = string.Empty; // Hướng dẫn sử dụng (VD: Sau ăn, trước ngủ)
    }
}

