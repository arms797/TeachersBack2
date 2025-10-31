using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.Models;

namespace TeachersBack2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class CenterController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CenterController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Center
        [HttpGet]
        [AllowAnonymous] // اگر بخوای لیست مراکز برای همه قابل دسترسی باشه
        public async Task<IActionResult> GetAll()
        {
            var centers = await _context.Centers
                .OrderBy(c => c.Title)
                .ToListAsync();

            return Ok(centers);
        }

        // GET: api/Center/101
        [HttpGet("{centerCode}")]
        public async Task<IActionResult> Get(string centerCode)
        {
            var center = await _context.Centers.FindAsync(centerCode);
            if (center == null)
                return NotFound(new { message = "مرکز مورد نظر یافت نشد." });

            return Ok(center);
        }

        // POST: api/Center
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Center model)
        {
            if (await _context.Centers.AnyAsync(c => c.CenterCode == model.CenterCode))
                return BadRequest(new { message = "مرکز با این کد قبلاً ثبت شده است." });

            _context.Centers.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // PUT: api/Center/101
        [HttpPut("{centerCode}")]
        public async Task<IActionResult> Update(string centerCode, [FromBody] Center model)
        {
            if (centerCode != model.CenterCode)
                return BadRequest(new { message = "کد مرکز با آدرس مطابقت ندارد." });

            var existing = await _context.Centers.FindAsync(centerCode);
            if (existing == null)
                return NotFound(new { message = "مرکز مورد نظر یافت نشد." });

            existing.Title = model.Title;
            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // DELETE: api/Center/101
        [HttpDelete("{centerCode}")]
        public async Task<IActionResult> Delete(string centerCode)
        {
            var center = await _context.Centers.FindAsync(centerCode);
            if (center == null)
                return NotFound(new { message = "مرکز مورد نظر یافت نشد." });

            _context.Centers.Remove(center);
            await _context.SaveChangesAsync();

            return Ok(new { message = "مرکز با موفقیت حذف شد." });
        }
    }
}
