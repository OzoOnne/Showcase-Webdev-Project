using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Showcase_API.Data;
using Showcase_API.Models;

namespace Showcase_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly FavoritesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public FavoritesController(FavoritesDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var list = await _db.Favorites
                .Where(f => f.UserId == user.Id)
                .Select(f => new { f.FestivalId, f.CreatedAt })
                .ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddFavoriteRequest req)
        {
            if (req == null) return BadRequest();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var exists = await _db.Favorites.AnyAsync(f => f.UserId == user.Id && f.FestivalId == req.FestivalId);
            if (exists) return Conflict("Already favorited");

            var fav = new Favorite { UserId = user.Id, FestivalId = req.FestivalId };
            _db.Favorites.Add(fav);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { festivalId = req.FestivalId }, new { req.FestivalId });
        }

        [HttpDelete("{festivalId:int}")]
        public async Task<IActionResult> Remove(int festivalId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var fav = await _db.Favorites.FirstOrDefaultAsync(f => f.UserId == user.Id && f.FestivalId == festivalId);
            if (fav == null) return NotFound();
            _db.Favorites.Remove(fav);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public record AddFavoriteRequest(int FestivalId);
}