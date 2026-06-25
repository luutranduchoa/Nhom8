using Medicine.DTOs;
using Medicine.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicine.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly MedicalDbContext _context;

        public AdminController(MedicalDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-drug")]
        public async Task<IActionResult> AddDrug([FromBody] CreateDrugDto request)
        {
            if (string.IsNullOrWhiteSpace(request.BrandName))
            {
                return BadRequest(new { Message = "Tên biệt dược không được để trống." });
            }

            // 1. Tạo và lưu Thuốc mới
            var newDrug = new Drug
            {
                BrandName = request.BrandName,
                GenericName = request.GenericName,
                DosageForm = request.DosageForm,
                CategoryId = request.CategoryId
            };

            _context.Drugs.Add(newDrug);
            await _context.SaveChangesAsync(); // Lưu để EF Core tự sinh ra DrugId

            // 2. Xử lý danh sách Chỉ định (Bệnh được điều trị)
            if (request.Indications != null && request.Indications.Any())
            {
                foreach (var ind in request.Indications)
                {
                    if (string.IsNullOrWhiteSpace(ind.Icd10Code)) continue;

                    // Tìm xem bệnh này (theo mã ICD-10) đã tồn tại trong DB chưa
                    var existingDisease = await _context.Diseases
                        .FirstOrDefaultAsync(d => d.Icd10Code.ToLower() == ind.Icd10Code.ToLower());

                    // Nếu bệnh chưa tồn tại, thêm bệnh mới vào DB
                    if (existingDisease == null)
                    {
                        existingDisease = new Disease
                        {
                            Icd10Code = ind.Icd10Code.ToUpper(),
                            Name = ind.DiseaseName
                        };
                        _context.Diseases.Add(existingDisease);
                        await _context.SaveChangesAsync(); // Lưu để lấy DiseaseId
                    }

                    // 3. Liên kết Thuốc và Bệnh vào bảng trung gian
                    var indication = new Indication
                    {
                        DrugId = newDrug.Id,
                        DiseaseId = existingDisease.Id,
                        DosageInstruction = ind.DosageInstruction
                    };
                    _context.Indications.Add(indication);
                }

                await _context.SaveChangesAsync(); // Lưu tất cả liên kết
            }

            if (request.Contraindications != null && request.Contraindications.Any())
            {
                foreach (var contra in request.Contraindications)
                {
                    if (string.IsNullOrWhiteSpace(contra.Icd10Code)) continue;

                    var existingDisease = await _context.Diseases
                        .FirstOrDefaultAsync(d => d.Icd10Code.ToLower() == contra.Icd10Code.ToLower());

                    if (existingDisease == null)
                    {
                        existingDisease = new Disease { Icd10Code = contra.Icd10Code.ToUpper(), Name = contra.DiseaseName };
                        _context.Diseases.Add(existingDisease);
                        await _context.SaveChangesAsync();
                    }

                    var contraindication = new Contraindication
                    {
                        DrugId = newDrug.Id,
                        DiseaseId = existingDisease.Id,
                        WarningNotes = contra.WarningNotes
                    };
                    _context.Contraindications.Add(contraindication);
                }
                await _context.SaveChangesAsync();
            }
            return Ok(new { Message = "Đã lưu Thuốc và Chỉ định thành công!", DrugId = newDrug.Id });
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "Không có file được chọn." });

            // 1. Tạo thư mục wwwroot/uploads nếu chưa tồn tại
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // 2. Đổi tên file để tránh bị trùng lặp (dùng mã Guid)
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 3. Copy file từ request vào thư mục vật lý trên ổ cứng
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. Tạo đường dẫn URL tĩnh để trả về cho Frontend
            var request = HttpContext.Request;
            var imageUrl = $"{request.Scheme}://{request.Host}/uploads/{uniqueFileName}";

            return Ok(new { ImageUrl = imageUrl });
        }
        // 1. API LẤY DANH SÁCH TẤT CẢ CÁC THUỐC
        [HttpGet("all-drugs")]
        public async Task<IActionResult> GetAllDrugs()
        {
            var drugs = await _context.Drugs
                .OrderByDescending(d => d.Id) // Xếp thuốc mới thêm lên đầu
                .Select(d => new
                {
                    d.Id,
                    d.BrandName,
                    d.GenericName,
                    d.DosageForm,
                    d.ImageUrl
                })
                .ToListAsync();

            return Ok(drugs);
        }

        // 2. API XÓA THUỐC
        [HttpDelete("delete-drug/{id}")]
        public async Task<IActionResult> DeleteDrug(int id)
        {
            // Bao gồm cả Indications và Contraindications để Entity Framework tự động xóa các liên kết
            var drug = await _context.Drugs
                .Include(d => d.Indications)
                .Include(d => d.Contraindications)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (drug == null)
            {
                return NotFound(new { Message = "Không tìm thấy thuốc này trong hệ thống." });
            }

            // (Tùy chọn) Xóa luôn file ảnh trong thư mục wwwroot để dọn rác ổ cứng
            if (!string.IsNullOrEmpty(drug.ImageUrl))
            {
                var fileName = Path.GetFileName(new Uri(drug.ImageUrl).LocalPath);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Xóa khỏi Database
            _context.Drugs.Remove(drug);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã xóa thuốc và các dữ liệu liên quan thành công!" });
        }
        // 1. API LẤY THÔNG TIN CỦA 1 THUỐC ĐỂ ĐIỀN VÀO FORM
        [HttpGet("get-drug/{id}")]
        public async Task<IActionResult> GetDrugById(int id)
        {
            var drug = await _context.Drugs
                .Include(d => d.Indications).ThenInclude(i => i.Disease)
                .Include(d => d.Contraindications).ThenInclude(c => c.Disease)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (drug == null) return NotFound(new { Message = "Không tìm thấy thuốc." });

            var result = new
            {
                drug.Id,
                drug.BrandName,
                drug.GenericName,
                drug.DosageForm,
                drug.ImageUrl,
                Indications = drug.Indications.Select(i => new {
                    i.Disease.Icd10Code,
                    DiseaseName = i.Disease.Name,
                    i.DosageInstruction
                }),
                Contraindications = drug.Contraindications.Select(c => new {
                    c.Disease.Icd10Code,
                    DiseaseName = c.Disease.Name,
                    c.WarningNotes
                })
            };

            return Ok(result);
        }

        // 2. API CẬP NHẬT THUỐC (Dùng chung CreateDrugDto cho tiện)
        [HttpPut("edit-drug/{id}")]
        public async Task<IActionResult> EditDrug(int id, [FromBody] CreateDrugDto request)
        {
            var drug = await _context.Drugs
                .Include(d => d.Indications)
                .Include(d => d.Contraindications)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (drug == null) return NotFound(new { Message = "Không tìm thấy thuốc." });

            // Cập nhật thông tin cơ bản
            drug.BrandName = request.BrandName;
            drug.GenericName = request.GenericName;
            drug.DosageForm = request.DosageForm;
            drug.CategoryId = request.CategoryId;
            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                drug.ImageUrl = request.ImageUrl; // Chỉ đè ảnh mới nếu có tải lên
            }

            // Xóa sạch các chỉ định/chống chỉ định cũ của thuốc này
            _context.Indications.RemoveRange(drug.Indications);
            _context.Contraindications.RemoveRange(drug.Contraindications);
            await _context.SaveChangesAsync();

            // --- THÊM LẠI CHỈ ĐỊNH MỚI ---
            if (request.Indications != null)
            {
                foreach (var ind in request.Indications)
                {
                    if (string.IsNullOrWhiteSpace(ind.Icd10Code)) continue;
                    var existingDisease = await _context.Diseases.FirstOrDefaultAsync(d => d.Icd10Code.ToLower() == ind.Icd10Code.ToLower());
                    if (existingDisease == null)
                    {
                        existingDisease = new Disease { Icd10Code = ind.Icd10Code.ToUpper(), Name = ind.DiseaseName };
                        _context.Diseases.Add(existingDisease);
                        await _context.SaveChangesAsync();
                    }
                    _context.Indications.Add(new Indication { DrugId = drug.Id, DiseaseId = existingDisease.Id, DosageInstruction = ind.DosageInstruction });
                }
            }

            // --- THÊM LẠI CHỐNG CHỈ ĐỊNH MỚI ---
            if (request.Contraindications != null)
            {
                foreach (var contra in request.Contraindications)
                {
                    if (string.IsNullOrWhiteSpace(contra.Icd10Code)) continue;
                    var existingDisease = await _context.Diseases.FirstOrDefaultAsync(d => d.Icd10Code.ToLower() == contra.Icd10Code.ToLower());
                    if (existingDisease == null)
                    {
                        existingDisease = new Disease { Icd10Code = contra.Icd10Code.ToUpper(), Name = contra.DiseaseName };
                        _context.Diseases.Add(existingDisease);
                        await _context.SaveChangesAsync();
                    }
                    _context.Contraindications.Add(new Contraindication { DrugId = drug.Id, DiseaseId = existingDisease.Id, WarningNotes = contra.WarningNotes });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Đã cập nhật thông tin thuốc thành công!" });
        }
    }
}
