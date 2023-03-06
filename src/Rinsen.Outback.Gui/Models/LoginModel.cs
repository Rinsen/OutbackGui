using System.ComponentModel.DataAnnotations;

namespace Rinsen.Outback.Gui.Models;

public class LoginModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public bool InvalidEmailOrPassword { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;

}
