using System;
using System.Text.Json.Serialization;

namespace Rinsen.IdentityProvider.Outback.Entities;

public class OutbackClientLoginRedirectUri : ICreatedAndUpdatedTimestamp, ISoftDelete
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string LoginRedirectUri { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Updated { get; set; }

    public DateTimeOffset? Deleted { get; set; }


    [JsonIgnore]
    public virtual OutbackClient? Client { get; set; }
}
