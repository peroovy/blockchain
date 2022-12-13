using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using Base58Check;
using Org.BouncyCastle.Asn1.Ocsp;
using XC.RSAUtil;

namespace Core.Utils;

public static class RsaUtils
{
    private static HashAlgorithmName signatureAlgorithm = HashAlgorithmName.SHA256;
    private static RSASignaturePadding signaturePadding = RSASignaturePadding.Pkcs1;
    
    private static readonly byte[] Version = { 0 };
    
    private const int ChecksumLength = 4;

    public static (string privateKey, string publicKey) GenerateRsaPair()
    {
        var values = RsaKeyGenerator.Pkcs1Key(2048, true);

        return (values[0], values[1]);
    }

    public static string SignData(string privateKey, string data)
    {
        return new RsaPkcs1Util(Encoding.UTF8, null, RsaPemFormatHelper.Pkcs1PrivateKeyFormatRemove(privateKey))
            .SignData(data, signatureAlgorithm, signaturePadding);
    }
    
    public static bool VerifyData(string publicKey, string signature, string data)
    {
        return new RsaPkcs1Util(Encoding.UTF8, RsaPemFormatHelper.PublicKeyFormatRemove(publicKey))
            .VerifyData(data, signature, signatureAlgorithm, signaturePadding);
    }
    
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