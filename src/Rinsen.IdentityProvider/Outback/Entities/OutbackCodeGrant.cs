using System;

namespace Rinsen.IdentityProvider.Outback.Entities;

public class OutbackCodeGrant : ICreatedTimestamp
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public Guid SubjectId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string CodeChallange { get; set; } = string.Empty;

    public string CodeChallangeMethod { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Nonce { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Resolved { get; set; }

    public DateTimeOffset Expires { get; set; }

    public virtual OutbackClient? Client { get; set; }
}
