using System;

namespace Rinsen.IdentityProvider.Settings;

public class Setting
{
    public int Id { get; set; }

    public Guid IdentityId { get; set; }

    public string KeyField { get; set; } = string.Empty;

    public string ValueField { get; set; } = string.Empty;

    public DateTimeOffset Accessed { get; set; }


}
