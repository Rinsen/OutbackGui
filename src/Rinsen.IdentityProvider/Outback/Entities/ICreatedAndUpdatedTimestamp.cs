using System;

namespace Rinsen.IdentityProvider.Outback.Entities;

public interface ICreatedAndUpdatedTimestamp
{
    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Updated { get; set; }

}
