namespace Medicine.Models
{
    public class PrescriptionDetail
    {
        public int Id { get; set; }
        
        public int PrescriptionId { get; set; }
        public Prescription? Prescription { get; set; }

        public int DrugId { get; set; }
        public Drug? Drug { get; set; }

        public int Quantity { get; set; }
        public string DosageInstruction { get; set; } = string.Empty; // Căn dặn, liều lượng (VD: Sáng 1 viên, tối 1 viên)
    }
}
