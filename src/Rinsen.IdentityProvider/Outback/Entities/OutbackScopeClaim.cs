using System;
using System.Text.Json.Serialization;

namespace Rinsen.IdentityProvider.Outback.Entities;

public class OutbackScopeClaim
{
    public int Id { get; set; }

    public int ScopeId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Updated { get; set; }

    public DateTimeOffset? Deleted { get; set; }

    [JsonIgnore]
    public virtual OutbackScope? Scope { get; set; }
}
