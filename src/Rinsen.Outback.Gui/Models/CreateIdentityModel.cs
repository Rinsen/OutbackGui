using System.ComponentModel.DataAnnotations;

namespace Rinsen.Outback.Gui.Models;

public class CreateIdentityModel
{
    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress), Required(ErrorMessage = "The email field is required"), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Given name")]
    [Required(ErrorMessage = "The given name field is required")]
    public string GivenName { get; set; } = string.Empty;

    [Display(Name = "Surname")]
    [Required(ErrorMessage = "The last name field is required")]
    public string Surname { get; set; } = string.Empty;

    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Password")]
    [Required(ErrorMessage = "The password field is required"), DataType(DataType.Password), Compare(nameof(ConfirmPassword))]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Confirm password")]
    [Required(ErrorMessage = "The confirmed password field is required"), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Invite Code")]
    [Required(ErrorMessage = "Invite code is required")]
    public string InviteCode { get; set; } = string.Empty;

    public string RedirectUrl { get; internal set; } = string.Empty;

}
