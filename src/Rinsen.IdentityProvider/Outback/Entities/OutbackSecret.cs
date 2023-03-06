using System;

namespace Rinsen.IdentityProvider.Outback.Entities;

public class OutbackSecret : ICreatedAndUpdatedTimestamp, ISoftDelete
{
    public int Id { get; set; }

    public string CryptographyData { get; set; } = string.Empty;

    public PublicKeyCryptographyType PublicKeyCryptographyType { get; set; }

    public bool ActiveSigningKey { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Updated { get; set; }

    public DateTimeOffset? Deleted { get; set; }

    public DateTime Expires { get; set; }
}

public enum PublicKeyCryptographyType : byte
{
    EC_NistP256 = 1,

}
