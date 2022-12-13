using System.Linq;
using Base58Check;

namespace Core.Utils;

public static class RsaUtils
{
    private static readonly byte[] Version = { 0 };
    private const int ChecksumLength = 4;
    
    public static byte[] HashPublicKey(string key) => Hashing.SumRipemd160(Hashing.SumSha256(key));

    public static bool ValidateAddress(string address)
    {
        var payload = Base58CheckEncoding.DecodePlain(address);
        var version = new[] { payload[0] };
        var publicKeyHash = payload.Skip(1).Take(payload.Length - ChecksumLength - 1);
        var checksum = payload.Reverse().Take(ChecksumLength).Reverse().ToArray();

        var expectedChecksum = CalculateChecksum(version.Concat(publicKeyHash).ToArray());

        return checksum.SequenceEqual(expectedChecksum);
    }

    public static string GetPublicKeyHashFromAddress(string address)
    {
        var payload = Base58CheckEncoding.DecodePlain(address);

        return payload
            .Skip(1)
            .Take(payload.Length - ChecksumLength - 1)
            .ToHexDigest();
    }
    
    public static string GetAddressFromPublicKey(string publicKey)
    {
        var publicHash = RsaUtils.HashPublicKey(publicKey);
        
        var hashWithVersion = Version
            .Concat(publicHash)
            .ToArray();
        
        var checksum = CalculateChecksum(hashWithVersion);

        var payload = hashWithVersion
            .Concat(checksum)
            .ToArray();
        
        return Base58CheckEncoding.EncodePlain(payload);
    }
    
    private static byte[] CalculateChecksum(byte[] bytes)
    {
        return Hashing.SumSha256(Hashing.SumSha256(bytes))
            .Take(ChecksumLength)
            .ToArray();
    }
}