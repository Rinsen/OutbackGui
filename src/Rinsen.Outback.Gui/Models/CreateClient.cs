using System.ComponentModel.DataAnnotations;
using Rinsen.Outback.Clients;

namespace Rinsen.Outback.Gui.Models;

public class CreateClient
{
    [Required]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public ClientType ClientType { get; set; }

    [Required]
    public int FamilyId { get; set; }
}
