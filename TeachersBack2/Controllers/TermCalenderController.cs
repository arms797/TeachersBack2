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
    public class TermCalenderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TermCalenderController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TermCalender
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var terms = await _context.TermCalenders.OrderByDescending(t => t.Term).ToListAsync();
            return Ok(terms);
        }

        // GET: api/TermCalender/14031
        [HttpGet("{term}")]
        public async Task<IActionResult> Get(string term)
        {
            var item = await _context.TermCalenders.FindAsync(term);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // POST: api/TermCalender
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TermCalender model)
        {
            if (await _context.TermCalenders.AnyAsync(t => t.Term == model.Term))
                return BadRequest("ترم با این کد قبلاً ثبت شده است.");

            _context.TermCalenders.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        // PUT: api/TermCalender/14031
        [HttpPut("{term}")]
        public async Task<IActionResult> Update(string term, [FromBody] TermCalender model)
        {
            if (term != model.Term)
                return BadRequest("کد ترم با آدرس مطابقت ندارد.");

            var existing = await _context.TermCalenders.FindAsync(term);
            if (existing == null)
                return NotFound();

            existing.Title = model.Title;
            existing.Start = model.Start;
            existing.End = model.End;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // DELETE: api/TermCalender/14031
        [HttpDelete("{term}")]
        public async Task<IActionResult> Delete(string term)
        {
            var item = await _context.TermCalenders.FindAsync(term);
            if (item == null)
                return NotFound();

            _context.TermCalenders.Remove(item);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
