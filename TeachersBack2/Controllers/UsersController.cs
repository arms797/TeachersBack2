using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.Models;

namespace TeachersBack2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public UsersController(AppDbContext db, IConfiguration config)
    {
        _db = db; _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .ToListAsync();

        var result = users.Select(u => new
        {
            u.Id,
            u.FirstName,
            u.LastName,
            u.NationalCode,
            u.Mobile,
            u.Email,
            u.CenterCode,
            u.Username,
            u.IsActive,
            Roles = u.UserRoles.Select(r => r.Role.Title).ToList()
        });

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.NationalCode,
            user.Mobile,
            user.Email,
            user.CenterCode,
            user.Username,
            user.IsActive,
            Roles = user.UserRoles.Select(r => r.Role.Title).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User dto)
    {
        dto.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);
        _db.Users.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] User dto)
    {
        var user = await _db.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.NationalCode = dto.NationalCode;
        user.Mobile = dto.Mobile;
        user.Email = dto.Email;
        user.CenterCode = dto.CenterCode;
        user.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return Ok(user);
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        var newPass = "Temp12345";
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPass);
        await _db.SaveChangesAsync();

        return Ok(new { message = "رمز ریست شد", tempPassword = newPass });
    }
}
