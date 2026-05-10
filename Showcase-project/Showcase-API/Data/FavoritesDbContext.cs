using Microsoft.EntityFrameworkCore;
using Showcase_API.Models;

namespace Showcase_API.Data
{
    public class FavoritesDbContext : DbContext
    {
        public FavoritesDbContext(DbContextOptions<FavoritesDbContext> options) : base(options) { }

        public DbSet<Favorite> Favorites { get; set; } = null!;
    }
}