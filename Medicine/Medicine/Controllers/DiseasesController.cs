using Medicine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiseasesController : ControllerBase
    {
        private readonly MedicalDbContext _context;
        public DiseasesController(MedicalDbContext context) { _context = context; }

        // GET: api/Diseases
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Diseases.OrderBy(d => d.Name).ToListAsync();
            return Ok(list);
        }

        // GET: api/Diseases/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var disease = await _context.Diseases.FindAsync(id);
            if (disease == null) return NotFound("Không tìm thấy bệnh lý.");
            return Ok(disease);
        }

        // POST: api/Diseases
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Disease request)
        {
            if (request == null) return BadRequest("Dữ liệu không hợp lệ.");
            if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Tên bệnh không được để trống.");

            // optional: prevent duplicate by name (case-insensitive)
            var exists = await _context.Diseases.AnyAsync(d => d.Name.ToLower() == request.Name.ToLower());
            if (exists) return BadRequest("Bệnh lý đã tồn tại.");

            _context.Diseases.Add(request);
            await _context.SaveChangesAsync();
            return Ok(new Medicine.DTOs.ApiResponseDto { Message = "Thêm bệnh lý thành công.", Id = request.Id });
        }

        // PUT: api/Diseases/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Disease request)
        {
            if (request == null) return BadRequest("Dữ liệu không hợp lệ.");
            if (id != request.Id) return BadRequest("ID không khớp.");
            if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Tên bệnh không được để trống.");

            var disease = await _context.Diseases.FindAsync(id);
            if (disease == null) return NotFound("Không tìm thấy bệnh lý.");

            // update fields
            disease.Name = request.Name;
            disease.Description = request.Description;
            disease.Symptoms = request.Symptoms;
            disease.RecommendedMedications = request.RecommendedMedications;
            disease.TreatmentGuidelines = request.TreatmentGuidelines;
            disease.Prevention = request.Prevention;

            _context.Diseases.Update(disease);
            await _context.SaveChangesAsync();

            return Ok(disease);
        }

        // DELETE: api/Diseases/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var disease = await _context.Diseases.FindAsync(id);
            if (disease == null) return NotFound("Không tìm thấy bệnh lý.");

            // Optional: check for related indications/contraindications and prevent deletion if referenced
            var hasRefs = await _context.Indications.AnyAsync(i => i.DiseaseId == id) || await _context.Contraindications.AnyAsync(c => c.DiseaseId == id) || await _context.SuggestedProtocols.AnyAsync(p => p.DiseaseId == id);
            if (hasRefs)
            {
                return BadRequest("Không thể xóa bệnh lý vì đang được tham chiếu bởi dữ liệu khác.");
            }

            _context.Diseases.Remove(disease);
            await _context.SaveChangesAsync();
            return Ok(new Medicine.DTOs.ApiResponseDto { Message = "Đã xóa bệnh lý." });
        }
    }
}
