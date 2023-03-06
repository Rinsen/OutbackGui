using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rinsen.IdentityProvider.Outback.Entities;

public class OutbackClientFamily : ICreatedAndUpdatedTimestamp, ISoftDelete
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Updated { get; set; }

    public DateTimeOffset? Deleted { get; set; }

    [JsonIgnore]
    public virtual List<OutbackClient> Clients { get; set; } = new List<OutbackClient>();

}
