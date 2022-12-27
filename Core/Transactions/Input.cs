using System;
using Core.Utils;
using LiteDB;

namespace Core.Transactions;

[Serializable]
public class Input
{
    public Input(string outputHash, string publicKey)
    {
        OutputHash = outputHash;
        PublicKey = publicKey;
        Hash = Hashing
            .SumSha256(OutputHash, PublicKey, DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString())
            .ToHexDigest();
    }

    [BsonCtor]
    public Input() {}
    
    [BsonId]
    public int Id { get; set; }
    
    public string Hash { get; set; }

    public string OutputHash { get; set; }

    public string Signature { get; set; }

    public string PublicKey { get; set; }
}