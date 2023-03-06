using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rinsen.IdentityProvider.Outback.Entities;

public class OutbackScope : ICreatedAndUpdatedTimestamp, ISoftDelete
{
    public int Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ScopeName { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public bool ShowInDiscoveryDocument { get; set; }

    public bool ClaimsInIdToken { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Updated { get; set; }

    public DateTimeOffset? Deleted { get; set; }

    [JsonIgnore]
    public List<OutbackClientScope> ClientScopes { get; set; } = new List<OutbackClientScope>();

    public List<OutbackScopeClaim> ScopeClaims { get; set; } = new List<OutbackScopeClaim>();
}
