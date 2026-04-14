using Showcase_API.Models;
using System;
namespace Showcase_API.Data
{
    public static class DBSeeder
    {
        public static void Seed(AppDBContext context)
        {
            if (context.Festivals.Any())
            {
                return;
            }

            context.Festivals.AddRange(
                new Festival
                {
                    Name = "Defqon.1",
                    Location = "Biddinghuizen",
                    Date = new DateTime(2026, 6, 26, 0, 0, 0, DateTimeKind.Utc),
                    Genre = "Hardstyle",
                    Description = "DEFQON 1!!!"
                }
            );

            context.SaveChanges();
        }
    }
}

