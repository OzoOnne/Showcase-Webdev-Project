namespace Showcase_API.Models
{
    public class Festival
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Location { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
