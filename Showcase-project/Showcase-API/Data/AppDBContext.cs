using Showcase_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Showcase_API.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Festival> Festivals => Set<Festival>();
    }
}
