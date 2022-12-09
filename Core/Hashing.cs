using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Core;

public static class Hashing
{
    public static byte[] ToSHA256(params string[] values)
    {
        using var sha256 = SHA256.Create();

        var concatenated = new StringBuilder();
        foreach (var value in values)
            concatenated.Append(value);

        return sha256
            .ComputeHash(Encoding.UTF8.GetBytes(concatenated.ToString()))
            .ToArray();
    }

    public static IEnumerable<bool> ToBits(this byte[] bytes)
    {
        var bits = new BitArray(bytes);

        for (var i = 0; i < bits.Length; i++)
            yield return bits[i];
    }

    public static string ToHexDigest(this byte[] bytes)
    {
        var result = new StringBuilder();

        foreach (var b in bytes)
            result.Append(b.ToString("x2"));

        return result.ToString();
    }
}