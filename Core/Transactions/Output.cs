using System;
using Core.Utils;

namespace Core.Transactions;

[Serializable]
public class Output
{
    public Output(int value, string scriptPublicKey)
    {
        Value = value;
        ScriptPublicKey = scriptPublicKey;
        Hash = Hashing
            .SumSHA256(value.ToString(), scriptPublicKey)
            .ToHexDigest();
    }
    
    public string Hash { get; }
    
    public int Value { get; }
    
    public string ScriptPublicKey { get; }

    public bool BelongTo(string data) => ScriptPublicKey == data;
}