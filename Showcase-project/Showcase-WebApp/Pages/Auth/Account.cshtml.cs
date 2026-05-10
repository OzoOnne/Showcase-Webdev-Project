using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;
using Showcase_WebApp.Models;

namespace Showcase_WebApp.Pages.Auth;

public class AccountModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AccountModel> _logger;

    public AccountModel(IHttpClientFactory httpClientFactory, ILogger<AccountModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty]
    public LoginInputModel LoginInput { get; set; } = new();

    [BindProperty]
    public RegisterInputModel RegisterInput { get; set; } = new();

    public string Mode { get; set; } = "login";

    public void OnGet(string mode = "login") => Mode = mode;

    public async Task<IActionResult> OnPostLoginAsync()
    {
        Mode = "login";

        ModelState.Clear();
        if (!TryValidateModel(LoginInput, nameof(LoginInput)))
        {
            return Page();
        }

        var client = _httpClientFactory.CreateClient("FestivalApi");

        var response = await client.PostAsJsonAsync("api/auth/login", new
        {
            email = LoginInput.Email,
            password = LoginInput.Password
        });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Ongeldige inloggegevens.");
            _logger.LogWarning("Login failed for {Email}: {StatusCode}", LoginInput.Email, response.StatusCode);
            return Page();
        }

        // Try to read roles from the API response (adjust to your API shape)
        var content = await response.Content.ReadAsStringAsync();
        var roles = new List<string>();

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("roles", out var rolesElem) && rolesElem.ValueKind == JsonValueKind.Array)
            {
                foreach (var r in rolesElem.EnumerateArray())
                {
                    var role = r.GetString();
                    if (!string.IsNullOrEmpty(role)) roles.Add(role);
                }
            }
        }
        catch (JsonException) { /* ignore parse errors - roles empty */ }

        // Build claims (always include Name)
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, LoginInput.Email) };
        foreach (var role in roles.Distinct())
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Sign in with cookie
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        _logger.LogInformation("User logged in: {Email} (roles: {Roles})", LoginInput.Email, string.Join(",", roles));
        return RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        Mode = "register";

        ModelState.Clear();
        if (!TryValidateModel(RegisterInput, nameof(RegisterInput)))
        {
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("FestivalApi");

            var response = await client.PostAsJsonAsync("api/auth/register", new
            {
                email = RegisterInput.Email,
                password = RegisterInput.Password
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Registreren is mislukt: {error}");
                _logger.LogWarning("Register failed for {Email}: {StatusCode} - {Error}", RegisterInput.Email, response.StatusCode, error);
                return Page();
            }

            _logger.LogInformation("User registered: {Email}", RegisterInput.Email);
            return RedirectToPage("/Auth/Account", new { mode = "login" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during registration for {Email}", RegisterInput.Email);
            ModelState.AddModelError(string.Empty, "Er is een fout opgetreden tijdens registreren. Probeer het later opnieuw.");
            return Page();
        }
    }
}