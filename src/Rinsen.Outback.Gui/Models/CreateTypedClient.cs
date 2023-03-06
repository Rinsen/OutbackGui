using System.ComponentModel.DataAnnotations;

namespace Rinsen.Outback.Gui.Models;

public class CreateTypedClient
{
    [Required]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;


}
