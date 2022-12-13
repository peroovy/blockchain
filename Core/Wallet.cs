using System.Linq;
using Base58Check;
using Core.Utils;

namespace Core;

public class Wallet
{
    public Wallet(string privateKey, string publicKey)
    {
        PrivateKey = privateKey;
        PublicKey = publicKey;
        PublicKeyHash = RsaUtils.HashPublicKey(PublicKey).ToHexDigest();
        Address = RsaUtils.GetAddressFromPublicKey(PublicKey);
    }
    
    public string PrivateKey { get; }

    public string PublicKey { get; }
    
    public string PublicKeyHash { get; }
    
    public string Address { get; }
}