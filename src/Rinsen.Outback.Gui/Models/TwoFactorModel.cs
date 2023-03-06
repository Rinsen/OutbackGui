using System.ComponentModel.DataAnnotations;
using Rinsen.IdentityProvider.LocalAccounts;

namespace Rinsen.Outback.Gui.Models;

public class TwoFactorModel
{
    public string ReturnUrl { get; set; } = string.Empty;
    public bool TwoFactorEmailEnabled { get; set; }
    public bool TwoFactorSmsEnabled { get; set; }
    public bool TwoFactorTotpEnabled { get; set; }
    public bool TwoFactorAppNotificationEnabled { get; set; }
    public TwoFactorType TypeSelected { get; set; }
    [Required]
    public string KeyCode { get; set; } = string.Empty;
    public bool RememberMe { get; set; }

}
