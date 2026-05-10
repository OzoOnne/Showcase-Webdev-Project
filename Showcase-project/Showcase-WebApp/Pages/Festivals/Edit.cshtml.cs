using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Showcase_WebApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace Showcase_WebApp.Pages.Festivals
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public FestivalDto Festival { get; set; } = new();

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("FestivalApi");
            var festival = await client.GetFromJsonAsync<FestivalDto>($"api/festival/{id}");
            if (festival == null)
            {
                return NotFound();
            }

            Festival = festival;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _httpClientFactory.CreateClient("FestivalApi");

            Festival.Date = DateTime.SpecifyKind(Festival.Date, DateTimeKind.Utc);

            var response = await client.PutAsJsonAsync($"api/festival/{Festival.Id}", Festival);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Er ging iets mis bij het updaten van het festival.");
                return Page();
            }

            return RedirectToPage("/Index");
        }
    }
}
