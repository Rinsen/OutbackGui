using System;

namespace Rinsen.IdentityProvider.LocalAccounts;

public class UsedTotpLog
{
    public int Id { get; set; }

    public Guid IdentityId { get; set; }

    public string Code { get; set; } = string.Empty;

    public DateTimeOffset UsedTime { get; set; }

}
