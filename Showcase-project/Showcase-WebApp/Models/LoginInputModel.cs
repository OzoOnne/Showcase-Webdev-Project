using System.ComponentModel.DataAnnotations;

namespace Showcase_WebApp.Models
{
    public class LoginInputModel
    {
        [Required(ErrorMessage = "E-mailadres is verplicht.")]
        [EmailAddress(ErrorMessage = "Vul een geldig e-mailadres in.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wachtwoord is verplicht.")]
        public string Password { get; set; } = string.Empty;
    }
}
