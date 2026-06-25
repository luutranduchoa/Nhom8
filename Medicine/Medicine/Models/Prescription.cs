using System;
using System.Collections.Generic;

namespace Medicine.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        public string PrescriptionCode { get; set; } = string.Empty; // Mã đơn thuốc
        public string PatientName { get; set; } = string.Empty; // Tên bệnh nhân
        public string PatientPhone { get; set; } = string.Empty;
        public string PatientAge { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty; // Chẩn đoán
        public string CreatedBy { get; set; } = string.Empty; // Tên bác sĩ lập đơn
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? Notes { get; set; }

        // Navigation properties
        public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
    }
}
