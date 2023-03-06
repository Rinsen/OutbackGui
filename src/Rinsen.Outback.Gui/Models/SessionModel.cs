using System;

namespace Rinsen.Outback.Gui.Models;

public class SessionModel
{
    public int Id { get; set; }
    public string ClientDescrition { get; internal set; } = string.Empty;
    public DateTimeOffset Created { get; internal set; }
    public DateTimeOffset Expires { get; internal set; }
    public string IpAddress { get; internal set; } = string.Empty;
    public bool Deleted { get; internal set; }
    public bool Expired { get; internal set; }

}
