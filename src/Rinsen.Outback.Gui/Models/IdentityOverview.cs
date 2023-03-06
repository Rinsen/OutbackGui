using System.Collections.Generic;

namespace Rinsen.Outback.Gui.Models;

public class IdentityOverview
{
    public SessionModel CurrentSession { get; set; } = new SessionModel();

    public List<SessionModel> Sessions { get; internal set; } = new List<SessionModel>();
}
