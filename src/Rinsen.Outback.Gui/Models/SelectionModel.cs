using System.Collections.Generic;
using System.Linq;

namespace Rinsen.Outback.Gui.Models;

public class SelectionModel
{
    public IEnumerable<int> LogLevels { get; set; } = Enumerable.Empty<int>();

    public IEnumerable<int> LogEnvironments { get; set; } = Enumerable.Empty<int>();

    public IEnumerable<int> LogApplications { get; set; } = Enumerable.Empty<int>();

    public IEnumerable<int> LogSources { get; set; } = Enumerable.Empty<int>();
}
