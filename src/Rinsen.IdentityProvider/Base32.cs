using System;

namespace Rinsen.IdentityProvider;

public class Base32
{
    const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567=";

    public static string Encode(byte[] bytes)
    {
        if (bytes == null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }

        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        var octetGroups = bytes.Length / 5;
        var padding = bytes.Length % 5;

        if (padding > 0)
            octetGroups++;

        var result = new char[octetGroups * 8];
        var resultSpan = result.AsSpan();
        var bytesSpan = bytes.AsSpan();

        for (int i = 0; i < octetGroups; i++)
        {
            var length = GetSpliceLength(bytesSpan, i);
            
            ProcessOctetGroup(resultSpan.Slice(i * 8, 8), bytesSpan.Slice(i * 5, length));
        }

        return new string(result);
    }

    private static int GetSpliceLength(Span<byte> bytesSpan, int i)
    {
        if (i * 5 + 5 > bytesSpan.Length)
        {
            return bytesSpan.Length - i * 5;
        }

        return 5;
    }

    private static void ProcessOctetGroup(Span<char> chars, Span<byte> bytes)
    {
        var charIndexer = (bytes[0] & 0xF8) >> 3;
        chars[0] = alphabet[charIndexer];

        charIndexer = (bytes[0] & 0x07) << 2;
        if (bytes.Length == 1)
        {
            chars[1] = alphabet[charIndexer];
            chars[2] = alphabet[32];
            chars[3] = alphabet[32];
            chars[4] = alphabet[32];
            chars[5] = alphabet[32];
            chars[6] = alphabet[32];
            chars[7] = alphabet[32];
            return;
        }

        charIndexer = charIndexer | ((bytes[1] & 0xC0) >> 6);
        chars[1] = alphabet[charIndexer];

        charIndexer = (bytes[1] & 0x3E) >> 1;
        chars[2] = alphabet[charIndexer];

        charIndexer = (bytes[1] & 0x01) << 4;

        if (bytes.Length == 2)
        {
            chars[3] = alphabet[charIndexer];
            chars[4] = alphabet[32];
            chars[5] = alphabet[32];
            chars[6] = alphabet[32];
            chars[7] = alphabet[32];
            return;
        }

        charIndexer = charIndexer | ((bytes[2] & 0xF0) >> 4);
        chars[3] = alphabet[charIndexer];

        charIndexer = (bytes[2] & 0x0F) << 1;

        if (bytes.Length == 3)
        {
            chars[4] = alphabet[charIndexer];
            chars[5] = alphabet[32];
            chars[6] = alphabet[32];
            chars[7] = alphabet[32];
            return;
        }

        charIndexer = charIndexer | ((bytes[3] & 0xF0) >> 4);
        chars[4] = alphabet[charIndexer];

        charIndexer = (bytes[3] & 0x7C) >> 2;
        chars[5] = alphabet[charIndexer];

        charIndexer = (bytes[3] & 0x03) << 3;

        if (bytes.Length == 4)
        {
            chars[6] = alphabet[charIndexer];
            chars[7] = alphabet[32];
            return;
        }

        charIndexer = charIndexer | ((bytes[4] & 0xE0) >> 5);
        chars[6] = alphabet[charIndexer];

        charIndexer = bytes[4] & 0x1F;
        chars[7] = alphabet[charIndexer];
    }
}

