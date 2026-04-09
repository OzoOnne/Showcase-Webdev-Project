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
    }
}
