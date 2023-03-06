using System.ComponentModel.DataAnnotations;

namespace Rinsen.Outback.Gui.Models;

public class CreateClientType
{

    [Required]
    public string DisplayName { get; set; } = string.Empty;

}
