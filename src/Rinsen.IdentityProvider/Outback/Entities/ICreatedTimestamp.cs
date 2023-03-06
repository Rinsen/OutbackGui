using System;

namespace Rinsen.IdentityProvider.Outback.Entities;

public interface ICreatedTimestamp
{
    public DateTimeOffset Created { get; set; }

}
