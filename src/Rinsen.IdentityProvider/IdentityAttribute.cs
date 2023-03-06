using System;

namespace Rinsen.IdentityProvider;

public class IdentityAttribute
{
    public int Id { get; set; }

    public Guid IdentityId { get; set; }

    public string Attribute { get; set; } = string.Empty;

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Updated { get; set; }

    public DateTimeOffset? Deleted { get; set; }
}
