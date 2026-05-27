using Medicine.DTOs;
using Medicine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medicine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly MedicalDbContext _context;

        public PrescriptionsController(MedicalDbContext context)
        {
            _context = context;
        }

        // GET: api/Prescriptions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrescriptionDto>>> GetPrescriptions()
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .ThenInclude(pd => pd.Drug)
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new PrescriptionDto
                {
                    Id = p.Id,
                    PatientName = p.PatientName,
                    Age = p.Age,
                    Diagnosis = p.Diagnosis,
                    CreatedDate = p.CreatedDate,
                    Notes = p.Notes,
                    Details = p.PrescriptionDetails.Select(pd => new PrescriptionDetailDto
                    {
                        DrugId = pd.DrugId,
                        DrugName = pd.Drug != null ? pd.Drug.BrandName : "",
                        Quantity = pd.Quantity,
                        DosageInstruction = pd.DosageInstruction
                    }).ToList()
                })
                .ToListAsync();

            return Ok(prescriptions);
        }

        // GET: api/Prescriptions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PrescriptionDto>> GetPrescription(int id)
        {
            var p = await _context.Prescriptions
                .Include(pr => pr.PrescriptionDetails)
                .ThenInclude(pd => pd.Drug)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (p == null)
            {
                return NotFound("Không tìm thấy đơn thuốc.");
            }

            var dto = new PrescriptionDto
            {
                Id = p.Id,
                PatientName = p.PatientName,
                Age = p.Age,
                Diagnosis = p.Diagnosis,
                CreatedDate = p.CreatedDate,
                Notes = p.Notes,
                Details = p.PrescriptionDetails.Select(pd => new PrescriptionDetailDto
                {
                    DrugId = pd.DrugId,
                    DrugName = pd.Drug != null ? pd.Drug.BrandName : "",
                    Quantity = pd.Quantity,
                    DosageInstruction = pd.DosageInstruction
                }).ToList()
            };

            // Kiểm tra tương tác thuốc trong đơn này
            var drugIds = dto.Details.Select(d => d.DrugId).ToList();
            var warnings = await CheckDrugInteractionsCore(drugIds);
            dto.Warnings = warnings;

            return Ok(dto);
        }

        // POST: api/Prescriptions
        [HttpPost]
        public async Task<ActionResult<PrescriptionDto>> CreatePrescription(CreatePrescriptionDto dto)
        {
            if (dto.Details == null || !dto.Details.Any())
            {
                return BadRequest("Đơn thuốc phải có ít nhất một loại thuốc.");
            }

            // Kiểm tra tương tác thuốc trước khi lưu
            var drugIds = dto.Details.Select(d => d.DrugId).ToList();
            var warnings = await CheckDrugInteractionsCore(drugIds);
            
            // Nếu có cảnh báo mức độ "Nặng" (Severe), có thể cấu hình để chặn tạo đơn 
            // (hoặc chỉ trả về cảnh báo tùy logic nghiệp vụ). Ở đây ta vẫn tạo nhưng cho biết cảnh báo.
            // Nếu muốn chặn:
            /*
            if (warnings.Any(w => w.IsSevere))
            {
                return BadRequest(new { Message = "Phát hiện tương tác thuốc đối kháng mức độ Nặng. Không thể tạo đơn thuốc.", Warnings = warnings });
            }
            */

            var prescription = new Prescription
            {
                PatientName = dto.PatientName,
                Age = dto.Age,
                Diagnosis = dto.Diagnosis,
                Notes = dto.Notes,
                CreatedDate = System.DateTime.UtcNow,
                PrescriptionDetails = dto.Details.Select(d => new PrescriptionDetail
                {
                    DrugId = d.DrugId,
                    Quantity = d.Quantity,
                    DosageInstruction = d.DosageInstruction
                }).ToList()
            };

            // Trừ tồn kho và ghi nhận lịch sử giao dịch (Export)
            foreach (var detail in dto.Details)
            {
                var drug = await _context.Drugs.FindAsync(detail.DrugId);
                if (drug != null)
                {
                    drug.StockQuantity -= detail.Quantity;
                    _context.Drugs.Update(drug);

                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        DrugId = detail.DrugId,
                        Quantity = detail.Quantity,
                        TransactionType = "Export",
                        Note = $"Xuất kho bán theo đơn cho bệnh nhân: {dto.PatientName}",
                        TransactionDate = System.DateTime.UtcNow
                    });
                }
            }

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            var result = await GetPrescription(prescription.Id);
            return CreatedAtAction(nameof(GetPrescription), new { id = prescription.Id }, result.Value);
        }

        // POST: api/Prescriptions/check-interactions
        // Endpoint riêng để tra cứu cảnh báo tương tác thuốc mà chưa cần tạo đơn
        [HttpPost("check-interactions")]
        public async Task<ActionResult<List<InteractionWarningDto>>> CheckInteractions([FromBody] List<int> drugIds)
        {
            var warnings = await CheckDrugInteractionsCore(drugIds);
            return Ok(warnings);
        }

        // GET: api/Prescriptions/suggested-protocol/{diseaseId}
        // Hiển thị đơn thuốc đề xuất dựa trên bệnh
        [HttpGet("suggested-protocol/{diseaseId}")]
        public async Task<ActionResult<SuggestedPrescriptionDto>> GetSuggestedPrescription(int diseaseId)
        {
            var disease = await _context.Diseases
                .Include(d => d.Indications)
                .ThenInclude(i => i.Drug)
                .FirstOrDefaultAsync(d => d.Id == diseaseId);

            if (disease == null)
            {
                return NotFound("Không tìm thấy bệnh.");
            }

            var suggested = new SuggestedPrescriptionDto
            {
                DiseaseId = disease.Id,
                DiseaseName = disease.Name,
                SuggestedDrugs = disease.Indications.Where(i => i.Drug != null)
                    .Select(i => new SuggestedDrugDto
                    {
                        DrugId = i.DrugId,
                        BrandName = i.Drug!.BrandName,
                        GenericName = i.Drug!.GenericName,
                        DosageInstruction = i.DosageInstruction
                    }).ToList()
            };

            return Ok(suggested);
        }

        // Hàm dùng chung để kiểm tra tương tác thuốc
        private async Task<List<InteractionWarningDto>> CheckDrugInteractionsCore(List<int> drugIds)
        {
            if (drugIds == null || drugIds.Count < 2)
            {
                return new List<InteractionWarningDto>();
            }

            var interactions = await _context.DrugInteractions
                .Include(di => di.SourceDrug)
                .Include(di => di.TargetDrug)
                .Where(di => drugIds.Contains(di.SourceDrugId) && drugIds.Contains(di.TargetDrugId))
                .ToListAsync();

            var warnings = interactions.Select(di => new InteractionWarningDto
            {
                SourceDrugName = di.SourceDrug.BrandName,
                TargetDrugName = di.TargetDrug.BrandName,
                Severity = di.Severity,
                Description = di.Description,
                // Mức độ Nặng/Đối kháng -> Hiển thị cảnh báo đỏ
                IsSevere = di.Severity.ToLower() == "nặng" || di.Severity.ToLower() == "severe" || di.Severity.ToLower() == "high" || di.Description.ToLower().Contains("đối kháng")
            }).ToList();

            return warnings;
        }
    }
}
