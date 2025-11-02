using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.DTOs;
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
        try
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
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {

        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
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
    public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
    {
        try
        {
            string pass;
            var isuser=await _db.Users.FirstOrDefaultAsync(x=>x.NationalCode==dto.NationalCode);
            if (isuser != null)
                return BadRequest("کد ملی تکراری است");
            if (dto.Password != null)
                pass = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            else
                pass = BCrypt.Net.BCrypt.HashPassword(dto.NationalCode != null ? dto.NationalCode : "Spnu123");
            User u = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                NationalCode = dto.NationalCode,
                Mobile = dto.Mobile,
                Email = dto.Email,
                CenterCode = dto.CenterCode,
                Username = dto.Username,
                IsActive = dto.IsActive,
                PasswordHash = pass
            };

            _db.Users.Add(u);
            await _db.SaveChangesAsync();
            return Ok(u);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] UserEditDto dto)
    {
        try
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
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        try
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var newPass = user.NationalCode != null ? user.NationalCode : "Spnu123";
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPass);
            await _db.SaveChangesAsync();

            return Ok(new { message = "رمز ریست شد", tempPassword = newPass });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
    [HttpPost("{id:int}/roles/{roleId:int}/")]
    public async Task<IActionResult> AddUserRole(int id,int roleId)
    {
        try
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            var role = await _db.Roles.FindAsync(roleId);
            if (role == null) return NotFound();
            var ur = await _db.UserRoles.Where(u => u.RoleId == roleId && u.UserId == id).FirstOrDefaultAsync();
            if (ur != null) return NotFound();
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };
            _db.UserRoles.Add(userRole);
            await _db.SaveChangesAsync();
            return Ok(userRole);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
    [HttpDelete("{id:int}/roles/{roleId:int}/")]
    public async Task<IActionResult> DeleteUserRole(int id, int roleId)
    {
        try
        {
            var userRole = await _db.UserRoles.Where(u => u.UserId == id && u.RoleId == roleId).FirstOrDefaultAsync();
            if (userRole == null) return NotFound();
            _db.UserRoles.Remove(userRole);
            await _db.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
    [HttpGet("{id:int}/roles")]
    public async Task<IActionResult> GetUserRoles(int id)
    {
        try
        {
            var userRole = await _db.UserRoles.Where(u => u.UserId == id).ToListAsync();
            if (userRole == null) return NotFound();
            return Ok(userRole);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
}
