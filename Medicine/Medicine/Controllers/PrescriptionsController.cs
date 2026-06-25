using Medicine.DTOs;
using Medicine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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

        /// <summary>
        /// Lấy danh sách tất cả đơn thuốc (Admin & User)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPrescriptions([FromQuery] string? createdBy = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest(new { Message = "PageNumber và PageSize phải >= 1" });

            var query = _context.Prescriptions
                .Where(p => !p.IsDeleted)
                .AsNoTracking();

            // Lọc theo bác sĩ nếu cần
            if (!string.IsNullOrWhiteSpace(createdBy))
            {
                query = query.Where(p => p.CreatedBy.ToLower().Contains(createdBy.ToLower()));
            }

            var totalCount = await query.CountAsync();
            var prescriptions = await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.PrescriptionDetails)
                .ThenInclude(d => d.Drug)
                .Select(p => new PrescriptionDto
                {
                    Id = p.Id,
                    PrescriptionCode = p.PrescriptionCode,
                    PatientName = p.PatientName,
                    PatientPhone = p.PatientPhone,
                    PatientAge = p.PatientAge,
                    Diagnosis = p.Diagnosis,
                    CreatedBy = p.CreatedBy,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    Notes = p.Notes,
                    PrescriptionDetails = p.PrescriptionDetails.Select(d => new PrescriptionDetailDto
                    {
                        Id = d.Id,
                        DrugId = d.DrugId,
                        DrugName = d.Drug.BrandName,
                        Dosage = d.Dosage,
                        Frequency = d.Frequency,
                        Duration = d.Duration,
                        Instructions = d.Instructions
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                totalCount = totalCount,
                pageNumber = pageNumber,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = prescriptions
            });
        }

        /// <summary>
        /// Lấy chi tiết một đơn thuốc
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPrescriptionById(int id)
        {
            var prescription = await _context.Prescriptions
                .Where(p => p.Id == id && !p.IsDeleted)
                .Include(p => p.PrescriptionDetails)
                .ThenInclude(d => d.Drug)
                .FirstOrDefaultAsync();

            if (prescription == null)
                return NotFound(new { Message = "Không tìm thấy đơn thuốc" });

            var result = new PrescriptionDto
            {
                Id = prescription.Id,
                PrescriptionCode = prescription.PrescriptionCode,
                PatientName = prescription.PatientName,
                PatientPhone = prescription.PatientPhone,
                PatientAge = prescription.PatientAge,
                Diagnosis = prescription.Diagnosis,
                CreatedBy = prescription.CreatedBy,
                CreatedDate = prescription.CreatedDate,
                UpdatedDate = prescription.UpdatedDate,
                Notes = prescription.Notes,
                PrescriptionDetails = prescription.PrescriptionDetails.Select(d => new PrescriptionDetailDto
                {
                    Id = d.Id,
                    DrugId = d.DrugId,
                    DrugName = d.Drug.BrandName,
                    Dosage = d.Dosage,
                    Frequency = d.Frequency,
                    Duration = d.Duration,
                    Instructions = d.Instructions
                }).ToList()
            };

            return Ok(result);
        }

        /// <summary>
        /// Tạo đơn thuốc mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePrescription([FromBody] CreatePrescriptionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.PatientName) || dto.Details.Count == 0)
                return BadRequest(new { Message = "Tên bệnh nhân và danh sách thuốc không được để trống" });

            // Kiểm tra các thuốc có tồn tại
            var drugIds = dto.Details.Select(d => d.DrugId).Distinct().ToList();
            var existingDrugs = await _context.Drugs
                .Where(d => drugIds.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync();

            if (existingDrugs.Count != drugIds.Count)
                return BadRequest(new { Message = "Một số thuốc không tồn tại trong hệ thống" });

            // Tạo mã đơn thuốc tự động
            var prescriptionCode = $"RX{DateTime.UtcNow:yyyyMMddHHmmss}";

            var prescription = new Prescription
            {
                PrescriptionCode = prescriptionCode,
                PatientName = dto.PatientName,
                PatientPhone = dto.PatientPhone,
                PatientAge = dto.PatientAge,
                Diagnosis = dto.Diagnosis,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                Notes = dto.Notes
            };

            foreach (var detailDto in dto.Details)
            {
                prescription.PrescriptionDetails.Add(new PrescriptionDetail
                {
                    DrugId = detailDto.DrugId,
                    Dosage = detailDto.Dosage,
                    Frequency = detailDto.Frequency,
                    Duration = detailDto.Duration,
                    Instructions = detailDto.Instructions
                });
            }

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPrescriptionById), new { id = prescription.Id }, new
            {
                Message = "Tạo đơn thuốc thành công",
                PrescriptionCode = prescriptionCode,
                Id = prescription.Id
            });
        }

        /// <summary>
        /// Cập nhật đơn thuốc
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrescription(int id, [FromBody] CreatePrescriptionDto dto)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (prescription == null)
                return NotFound(new { Message = "Không tìm thấy đơn thuốc" });

            // Kiểm tra các thuốc có tồn tại
            var drugIds = dto.Details.Select(d => d.DrugId).Distinct().ToList();
            var existingDrugs = await _context.Drugs
                .Where(d => drugIds.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync();

            if (existingDrugs.Count != drugIds.Count)
                return BadRequest(new { Message = "Một số thuốc không tồn tại trong hệ thống" });

            // Cập nhật thông tin
            prescription.PatientName = dto.PatientName;
            prescription.PatientPhone = dto.PatientPhone;
            prescription.PatientAge = dto.PatientAge;
            prescription.Diagnosis = dto.Diagnosis;
            prescription.CreatedBy = dto.CreatedBy;
            prescription.Notes = dto.Notes;
            prescription.UpdatedDate = DateTime.UtcNow;

            // Xóa chi tiết cũ
            _context.PrescriptionDetails.RemoveRange(prescription.PrescriptionDetails);

            // Thêm chi tiết mới
            foreach (var detailDto in dto.Details)
            {
                prescription.PrescriptionDetails.Add(new PrescriptionDetail
                {
                    DrugId = detailDto.DrugId,
                    Dosage = detailDto.Dosage,
                    Frequency = detailDto.Frequency,
                    Duration = detailDto.Duration,
                    Instructions = detailDto.Instructions
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cập nhật đơn thuốc thành công" });
        }

        /// <summary>
        /// Xóa (soft delete) đơn thuốc
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var prescription = await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (prescription == null)
                return NotFound(new { Message = "Không tìm thấy đơn thuốc" });

            prescription.IsDeleted = true;
            prescription.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xóa đơn thuốc thành công" });
        }

        /// <summary>
        /// Tìm kiếm đơn thuốc theo tên bệnh nhân hoặc mã đơn
        /// </summary>
        [HttpGet("search/{keyword}")]
        public async Task<IActionResult> SearchPrescriptions(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { Message = "Từ khóa tìm kiếm không được để trống" });

            var prescriptions = await _context.Prescriptions
                .Where(p => !p.IsDeleted &&
                    (p.PatientName.ToLower().Contains(keyword.ToLower()) ||
                     p.PrescriptionCode.ToLower().Contains(keyword.ToLower())))
                .Include(p => p.PrescriptionDetails)
                .ThenInclude(d => d.Drug)
                .Select(p => new PrescriptionDto
                {
                    Id = p.Id,
                    PrescriptionCode = p.PrescriptionCode,
                    PatientName = p.PatientName,
                    PatientPhone = p.PatientPhone,
                    PatientAge = p.PatientAge,
                    Diagnosis = p.Diagnosis,
                    CreatedBy = p.CreatedBy,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    Notes = p.Notes,
                    PrescriptionDetails = p.PrescriptionDetails.Select(d => new PrescriptionDetailDto
                    {
                        Id = d.Id,
                        DrugId = d.DrugId,
                        DrugName = d.Drug.BrandName,
                        Dosage = d.Dosage,
                        Frequency = d.Frequency,
                        Duration = d.Duration,
                        Instructions = d.Instructions
                    }).ToList()
                })
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            if (prescriptions.Count == 0)
                return NotFound(new { Message = "Không tìm thấy đơn thuốc phù hợp" });

            return Ok(prescriptions);
        }

        /// <summary>
        /// Lấy danh sách đơn thuốc theo bác sĩ
        /// </summary>
        [HttpGet("by-doctor/{doctorName}")]
        public async Task<IActionResult> GetPrescriptionsByDoctor(string doctorName)
        {
            var prescriptions = await _context.Prescriptions
                .Where(p => !p.IsDeleted && p.CreatedBy.ToLower() == doctorName.ToLower())
                .Include(p => p.PrescriptionDetails)
                .ThenInclude(d => d.Drug)
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new PrescriptionDto
                {
                    Id = p.Id,
                    PrescriptionCode = p.PrescriptionCode,
                    PatientName = p.PatientName,
                    PatientPhone = p.PatientPhone,
                    PatientAge = p.PatientAge,
                    Diagnosis = p.Diagnosis,
                    CreatedBy = p.CreatedBy,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    Notes = p.Notes,
                    PrescriptionDetails = p.PrescriptionDetails.Select(d => new PrescriptionDetailDto
                    {
                        Id = d.Id,
                        DrugId = d.DrugId,
                        DrugName = d.Drug.BrandName,
                        Dosage = d.Dosage,
                        Frequency = d.Frequency,
                        Duration = d.Duration,
                        Instructions = d.Instructions
                    }).ToList()
                })
                .ToListAsync();

            if (prescriptions.Count == 0)
                return NotFound(new { Message = $"Không tìm thấy đơn thuốc của bác sĩ {doctorName}" });

            return Ok(prescriptions);
        }

        /// <summary>
        /// Kiểm tra tương tác thuốc trong đơn
        /// </summary>
        [HttpPost("check-interactions")]
        public async Task<IActionResult> CheckInteractions([FromBody] List<int> drugIds)
        {
            if (drugIds == null || drugIds.Count < 2)
                return BadRequest(new { Message = "Cần ít nhất 2 loại thuốc để kiểm tra tương tác" });

            var warnings = new List<InteractionWarningDto>();

            // Kiểm tra tương tác từng cặp thuốc
            for (int i = 0; i < drugIds.Count; i++)
            {
                for (int j = i + 1; j < drugIds.Count; j++)
                {
                    var interaction = await _context.DrugInteractions
                        .Include(di => di.SourceDrug)
                        .Include(di => di.TargetDrug)
                        .FirstOrDefaultAsync(di =>
                            (di.SourceDrugId == drugIds[i] && di.TargetDrugId == drugIds[j]) ||
                            (di.SourceDrugId == drugIds[j] && di.TargetDrugId == drugIds[i])
                        );

                    if (interaction != null)
                    {
                        warnings.Add(new InteractionWarningDto
                        {
                            SourceDrugName = interaction.SourceDrug.BrandName,
                            TargetDrugName = interaction.TargetDrug.BrandName,
                            Severity = interaction.Severity,
                            Description = interaction.Description,
                            IsSevere = interaction.Severity.ToLower() == "nặng" || interaction.Severity.ToLower() == "severe"
                        });
                    }
                }
            }

            return Ok(new
            {
                HasWarnings = warnings.Count > 0,
                WarningCount = warnings.Count,
                Warnings = warnings
            });
        }
    }
}
