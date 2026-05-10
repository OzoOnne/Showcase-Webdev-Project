using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Showcase_WebApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace Showcase_WebApp.Pages.Festivals
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public FestivalDto Festival { get; set; } = new();

        public DeleteModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("FestivalApi");

            var festival = await client.GetFromJsonAsync<FestivalDto>($"api/festival/{id}");

            if (festival is null)
            {
                return NotFound();
            }

            Festival = festival;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("FestivalApi");

            var response = await client.DeleteAsync($"api/festival/{id}");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Er ging iets mis bij het verwijderen van het festival.");
                return Page();
            }

            return RedirectToPage("/Index");
        }
    }
}
