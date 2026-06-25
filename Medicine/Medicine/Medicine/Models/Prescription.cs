using System;
using System.Collections.Generic;

namespace Medicine.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
    }
}
