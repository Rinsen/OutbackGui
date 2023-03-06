using System.ComponentModel.DataAnnotations;

namespace Rinsen.Outback.Gui.Models;

public class CreateIdentityResource
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;


}
