using System;
using System.Collections.Generic;

namespace Medicine.Models
{
    public class SuggestedProtocol
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty; // Tiêu đề phác đồ/đơn thuốc đề xuất
        public int? DiseaseId { get; set; } // Có thể gắn với bệnh cụ thể hoặc không
        public Disease? Disease { get; set; }
        public string? Notes { get; set; } // Ghi chú thêm
        
        public ICollection<SuggestedProtocolDetail> Details { get; set; } = new List<SuggestedProtocolDetail>();
    }

    public class SuggestedProtocolDetail
    {
        public int Id { get; set; }
        public int ProtocolId { get; set; }
        public SuggestedProtocol? Protocol { get; set; }
        public int DrugId { get; set; }
        public Drug? Drug { get; set; }
        public string DosageInstruction { get; set; } = string.Empty; // Hướng dẫn sử dụng
    }
}
