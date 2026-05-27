using Medicine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly MedicalDbContext _context;
        public CategoriesController(MedicalDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _context.DrugCategories.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DrugCategory request)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Tên loại không được để trống.");
            _context.DrugCategories.Add(request);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Thêm loại thành công!" });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _context.DrugCategories.FindAsync(id);
            if (cat == null) return NotFound("Không tìm thấy loại này.");
            _context.DrugCategories.Remove(cat);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Đã xóa loại." });
        }


    }
}
