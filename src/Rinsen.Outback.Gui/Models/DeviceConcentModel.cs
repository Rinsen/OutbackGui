using System.Collections.Generic;

namespace Rinsen.Outback.Gui.Models;

public class DeviceConcentModel
{
    public string RememberConcent { get; set; } = string.Empty;

    public IEnumerable<string> ScopeConcented { get; set; } = new List<string>();

    public string AcceptButton { get; set; } = string.Empty;

    public string UserCode { get; set; } = string.Empty;

}
