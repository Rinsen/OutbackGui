using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;

namespace Rinsen.IdentityProvider;

public class RandomStringGenerator
{
    private readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();

    public string GetRandomString(int length)
    {
        var bytes = new byte[length + 10];

        CryptoRandom.GetBytes(bytes);

        return Base64UrlTextEncoder.Encode(bytes)[..length];
    }

}
