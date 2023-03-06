using System;
using System.Text.Json.Serialization;

namespace Rinsen.IdentityProvider.Outback.Entities;

public class OutbackClientScope : ICreatedAndUpdatedTimestamp, ISoftDelete
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int ScopeId { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Updated { get; set; }

    public DateTimeOffset? Deleted { get; set; }

    [JsonIgnore]
    public virtual OutbackClient? Client { get; set; }

    public virtual OutbackScope Scope { get; set; } = new OutbackScope();
}
