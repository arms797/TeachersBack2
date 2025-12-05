using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.Models;

[ApiController]
[Route("api/[controller]")]
public class ComponentFeatureController : ControllerBase
{
    private readonly AppDbContext _db;
    public ComponentFeatureController(AppDbContext db)
    {
        _db = db;
    }

    // خواندن همه کامپوننت‌ها
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var list = await _db.ComponentFeatures.ToListAsync();
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در دریافت داده‌ها: {ex.Message}");
        }
    }

    // خواندن یک کامپوننت با آیدی
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var feature = await _db.ComponentFeatures.FindAsync(id);
            if (feature == null) return NotFound("کامپوننت یافت نشد");
            return Ok(feature);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در دریافت داده: {ex.Message}");
        }
    }

    // ایجاد کامپوننت جدید
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ComponentFeature feature)
    {
        try
        {
            _db.ComponentFeatures.Add(feature);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = feature.Id }, feature);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در ایجاد کامپوننت: {ex.Message}");
        }
    }

    // ویرایش کامپوننت
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ComponentFeature updated)
    {
        try
        {
            var feature = await _db.ComponentFeatures.FindAsync(id);
            if (feature == null) return NotFound("کامپوننت یافت نشد");

            feature.Name = updated.Name;
            feature.Description = updated.Description;
            feature.IsActive = updated.IsActive;

            await _db.SaveChangesAsync();
            return Ok(feature);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در ویرایش کامپوننت: {ex.Message}");
        }
    }

    // فعال یا غیرفعال کردن کامپوننت
    [HttpPut("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id, [FromBody] bool isActive)
    {
        try
        {
            var feature = await _db.ComponentFeatures.FindAsync(id);
            if (feature == null) return NotFound("کامپوننت یافت نشد");

            feature.IsActive = isActive;
            await _db.SaveChangesAsync();
            return Ok(feature);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در تغییر وضعیت کامپوننت: {ex.Message}");
        }
    }
}
