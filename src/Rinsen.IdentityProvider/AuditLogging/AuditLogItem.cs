using System;

namespace Rinsen.IdentityProvider.AuditLogging;

public class AuditLogItem
{
    public int Id { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;

    public DateTimeOffset Timestamp { get; set; }

}
