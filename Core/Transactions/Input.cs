using System;
using Core.Utils;

namespace Core.Transactions;

[Serializable]
public class Input
{
    public Input(string previousTransactionHash, int outputIndex, string signature, string publicKey)
    {
        PreviousTransactionHash = previousTransactionHash;
        OutputIndex = outputIndex;
        Signature = signature;
        PublicKey = publicKey;
        Hash = Hashing
            .SumSha256(PreviousTransactionHash, OutputIndex.ToString(), Signature, PublicKey)
            .ToHexDigest();
    }
    
    public string PreviousTransactionHash { get; }
    
    public string Hash { get; }
    
    public int OutputIndex { get; }
    
    public string Signature { get; }
    
    public string PublicKey { get; }

    public bool BelongsTo(string publicKeyHash) => RsaUtils.HashPublicKey(PublicKey).ToHexDigest() == publicKeyHash;
}