using System;

namespace Rinsen.IdentityProvider.Outback.Entities;

public class OutbackPersistedGrant : ICreatedTimestamp
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public Guid SubjectId { get; set; }

    public string Scope { get; set; } = string.Empty;

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Expires { get; set; }

    public virtual OutbackClient? Client { get; set; }


}
