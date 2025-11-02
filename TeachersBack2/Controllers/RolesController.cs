using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.Models;

namespace TeachersBack2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RolesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var roles = await _db.Roles.ToListAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Role dto)
    {
        try
        {
            _db.Roles.Add(dto);
            await _db.SaveChangesAsync();
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
       
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] Role dto)
    {
        try
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null) return NotFound();

            role.Title = dto.Title;
            role.Description = dto.Description;
            await _db.SaveChangesAsync();

            return Ok(role);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null) return NotFound();

            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
            return Ok(new { message = "نقش حذف شد" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
}
