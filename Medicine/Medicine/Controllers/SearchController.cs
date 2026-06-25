using Medicine.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicine.Controllers
{
    
        [ApiController]
        [Route("api/[controller]")] // Route sẽ là: /api/search
        public class SearchController : ControllerBase
        {
            private readonly MedicalDbContext _context;

            // Tiêm MedicalDbContext qua Dependency Injection
            public SearchController(MedicalDbContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Tra cứu nhanh thông tin thuốc dựa trên tên biệt dược hoặc hoạt chất
            /// </summary>
            [HttpGet("quick-lookup")]
            public async Task<IActionResult> QuickLookupDrug([FromQuery] string keyword)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return BadRequest(new { Message = "Vui lòng nhập từ khóa tìm kiếm." });
                }

                keyword = keyword.ToLower().Trim();

                // Truy vấn và Map trực tiếp sang DTO
                var drugResult = await _context.Drugs
                    .AsNoTracking() // Tối ưu hiệu suất cho truy vấn chỉ đọc
                    .Where(d => d.BrandName.ToLower().Contains(keyword) ||
                                d.GenericName.ToLower().Contains(keyword))
                    .Select(d => new DrugDetailDto
                    {
                        Id = d.Id,
                        BrandName = d.BrandName,
                        GenericName = d.GenericName,
                        DosageForm = d.DosageForm,

                        // Lấy danh sách Chỉ định
                        Indications = d.Indications.Select(i => new DiseaseDto
                        {
                            Icd10Code = i.Disease.Icd10Code,
                            DiseaseName = i.Disease.Name,
                            Notes = i.DosageInstruction
                        }).ToList(),

                        // Lấy danh sách Chống chỉ định
                        Contraindications = d.Contraindications.Select(c => new DiseaseDto
                        {
                            Icd10Code = c.Disease.Icd10Code,
                            DiseaseName = c.Disease.Name,
                            Notes = c.WarningNotes
                        }).ToList(),

                        // Lấy danh sách Tương tác thuốc (ví dụ thuốc này là gốc)
                        Interactions = d.InteractionsAsSource.Select(interaction => new InteractionDto
                        {
                            SourceDrugName = d.BrandName,
                            TargetDrugName = interaction.TargetDrug.BrandName,
                            Severity = interaction.Severity,
                            Description = interaction.Description
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                // Xử lý nếu không tìm thấy thuốc
                if (drugResult == null)
                {
                    return NotFound(new { Message = "Không tìm thấy thông tin thuốc phù hợp." });
                }

                return Ok(drugResult);
            }
        // Trong một Controller mới hoặc SearchController
        [HttpPost("check-interactions")]
        public async Task<IActionResult> CheckInteractions([FromBody] List<int> drugIds)
        {
            if (drugIds == null || drugIds.Count < 2)
                return Ok(new List<InteractionDto>());

            // Tìm các cặp thuốc trong danh sách có tồn tại trong bảng DrugInteraction
            var interactions = await _context.DrugInteractions
                .Include(di => di.SourceDrug)
                .Include(di => di.TargetDrug)
                .Where(di => drugIds.Contains(di.SourceDrugId) && drugIds.Contains(di.TargetDrugId))
                .Select(di => new InteractionDto
                {
                    SourceDrugName = di.SourceDrug.BrandName,
                    TargetDrugName = di.TargetDrug.BrandName,
                    Severity = di.Severity, 
                    Description = di.Description
                })
                .ToListAsync();

            return Ok(interactions);
        }
    }
    
}
