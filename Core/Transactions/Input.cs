using System;
using Core.Utils;

namespace Core.Transactions;

[Serializable]
public class Input
{
    public Input(string outputHash, string publicKey)
    {
        OutputHash = outputHash;
        PublicKey = publicKey;
    }
    
    public string Hash => Hashing
        .SumSha256(OutputHash, PublicKey, Signature ?? string.Empty)
        .ToHexDigest();
    
    public string OutputHash { get; }
    
    public string Signature { get; set; }

    public string PublicKey { get; set; }
}