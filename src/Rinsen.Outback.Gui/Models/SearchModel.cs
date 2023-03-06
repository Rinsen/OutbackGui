using System;
using System.Collections.Generic;
using System.Linq;

namespace Rinsen.Outback.Gui.Models;

public class SearchModel
{
    public DateTimeOffset From { get; set; }

    public DateTimeOffset To { get; set; }

    public IEnumerable<int> LogLevels { get; set; } = Enumerable.Empty<int>();

    public IEnumerable<int> LogEnvironments { get; set; } = Enumerable.Empty<int>();

    public IEnumerable<int> LogApplications { get; set; } = Enumerable.Empty<int>();

    public IEnumerable<int> LogSources { get; set; } = Enumerable.Empty<int>();
}
