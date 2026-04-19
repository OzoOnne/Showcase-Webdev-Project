using Showcase_API.Models;
using Showcase_API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Showcase_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FestivalController : ControllerBase
    {
        private readonly AppDBContext _context;

        public FestivalController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Festival>>> GetFestivals()
        {
            var festivals = await _context.Festivals
            .OrderBy(f => f.Date)
            .ToListAsync();

            return Ok(festivals);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Festival>> GetFestival(int id)
        {
            var festival = await _context.Festivals.FindAsync(id);

            if (festival is null)
            {
                return NotFound();
            }

            return Ok(festival);
        }

        [HttpPost]
        public async Task<ActionResult<Festival>> CreateFestival(Festival festival)
        {
            _context.Festivals.Add(festival);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFestival), new { id = festival.Id }, festival);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFestival(int id, Festival updatedFestival)
        {
            if (id != updatedFestival.Id)
            {
                return BadRequest();
            }

            var existingFestival = await _context.Festivals.FindAsync(id);

            if (existingFestival is null)
            {
                return NotFound();
            }

            existingFestival.Name = updatedFestival.Name;
            existingFestival.Location = updatedFestival.Location;
            existingFestival.Date = updatedFestival.Date;
            existingFestival.Genre = updatedFestival.Genre;
            existingFestival.Description = updatedFestival.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFestival(int id)
        {
            var festival = await _context.Festivals.FindAsync(id);

            if (festival is null)
            {
                return NotFound();
            }

            _context.Festivals.Remove(festival);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
