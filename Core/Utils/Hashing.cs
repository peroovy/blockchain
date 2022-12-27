using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Core.Utils;

public static class Hashing
{
    public static string ZeroHash = new('0', 64);

    public static byte[] SumSha256(params string[] values) => SumSha256(values as IEnumerable<string>);

    public static byte[] SumSha256(IEnumerable<string> values)
    {
        var concatenated = new StringBuilder();
        foreach (var value in values)
            concatenated.Append(value);

        return SumSha256(Encoding.UTF8.GetBytes(concatenated.ToString()));
    }

    public static byte[] SumSha256(byte[] bytes)
    {
        using var sha256 = SHA256.Create();

        return sha256.ComputeHash(bytes);
    }

    public static byte[] SumRipemd160(byte[] bytes)
    {
        using var alg = RIPEMD160.Create();

        return alg.ComputeHash(bytes);
    }

    public static IEnumerable<bool> ToBits(this byte[] bytes)
    {
        var bits = new BitArray(bytes);

        for (var i = 0; i < bits.Length; i++)
            yield return bits[i];
    }

    public static IEnumerable<bool> ToBits(this string str) => ToBits(Encoding.UTF8.GetBytes(str));

    public static string ToHexDigest(this IEnumerable<byte> bytes)
    {
        var result = new StringBuilder();

        foreach (var b in bytes)
            result.Append(b.ToString("x2"));

        return result.ToString();
    }
}