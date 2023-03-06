using System;
using System.Collections.Generic;
using System.Linq;

namespace Rinsen.Outback.Gui.Models;

public class LoggerModel
{
    public SelectionOptions SelectionOptions { get; set; } = new SelectionOptions();

}


public class SelectionOptions
{
    public IEnumerable<SelectionLogApplication> LogApplications { get; set; } = Enumerable.Empty<SelectionLogApplication>();

    public IEnumerable<SelectionLogEnvironment> LogEnvironments { get; set; } = Enumerable.Empty<SelectionLogEnvironment>();

    public IEnumerable<SelectionLogLevel> LogLevels { get; set; } = Enumerable.Empty<SelectionLogLevel>();

    public IEnumerable<SelectionLogSource> LogSources { get; set; } = Enumerable.Empty<SelectionLogSource>();

    public DateTimeOffset From { get; set; }

    public DateTimeOffset To { get; set; }

}

public class SelectionLogApplication
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Selected { get; set; }
}

public class SelectionLogEnvironment
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Selected { get; set; }

}

public class SelectionLogLevel
{
    public int Level { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Selected { get; set; }

}

public class SelectionLogSource
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Selected { get; set; }

}
