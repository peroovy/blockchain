using System;
using Core.Utils;
using LiteDB;

namespace Core.Transactions;

[Serializable]
public class Output
{
    public Output(int value, string publicKeyHash, bool isSpent = false)
    {
        Value = value;
        PublicKeyHash = publicKeyHash;
        IsSpent = isSpent;
        Hash = Hashing
            .SumSha256(Value.ToString(), PublicKeyHash, DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString())
            .ToHexDigest();
    }
    
    [BsonCtor]
    public Output() {}

    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Hash { get; set; }
    
    public int Value { get; set; }
    
    public bool IsSpent { get; set; }
    
    public string PublicKeyHash { get; set; }
}