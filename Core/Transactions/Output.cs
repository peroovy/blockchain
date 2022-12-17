using System;
using Core.Utils;

namespace Core.Transactions;

public class Output
{
    public Output(int value, string publicKeyHash)
    {
        Value = value;
        PublicKeyHash = publicKeyHash;
        Hash = Hashing
            .SumSha256(Value.ToString(), PublicKeyHash)
            .ToHexDigest();
    }
    
    public string Hash { get; }
    
    public int Value { get; }
    
    public string PublicKeyHash { get; }
}