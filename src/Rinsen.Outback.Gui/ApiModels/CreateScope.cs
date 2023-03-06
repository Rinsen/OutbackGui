namespace Rinsen.Outback.Gui.ApiModels;

public class CreateScope
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ScopeName { get; set; } = string.Empty;

    public bool ShowInDiscoveryDocument { get; set; }

    public bool ClaimsInIdToken { get; set; }

    public bool Enabled { get; set; }

    public string Audience { get; set; } = string.Empty;
}
