using System;

namespace Showcase_API.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!; 
        public int FestivalId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}