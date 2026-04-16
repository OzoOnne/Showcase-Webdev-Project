using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Showcase_WebApp.Models;
using System.Net.Http.Json;

namespace Showcase_WebApp.Pages.Festivals;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    [BindProperty]
    public FestivalDto Festival { get; set; } = new();

    public CreateModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = _httpClientFactory.CreateClient("FestivalApi");

        Festival.Date = DateTime.SpecifyKind(Festival.Date, DateTimeKind.Utc);

        var response = await client.PostAsJsonAsync("api/festival", Festival);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Er ging iets mis bij het opslaan van het festival.");
            return Page();
        }

        return RedirectToPage("/Festivals/Index");
    }
}