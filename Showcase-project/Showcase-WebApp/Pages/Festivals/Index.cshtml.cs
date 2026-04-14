using System.Net.Http.Json;
using Showcase_WebApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Showcase_WebApp.Pages.Festivals
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public List<FestivalDto> Festivals { get; set; } = [];
        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("FestivalApi");
            var result = await client.GetFromJsonAsync<List<FestivalDto>>("api/festival");

            Festivals = result ?? [];
        }
    }
}
