using System;

namespace Rinsen.IdentityProvider;

public class Identity
{
    public int Id { get; set; }

    public Guid IdentityId { get; set; }

    public string GivenName { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool EmailConfirmed { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public bool PhoneNumberConfirmed { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Updated { get; set; }

    public DateTimeOffset? Deleted { get; set; }

}
