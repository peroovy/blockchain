using System;
using Core.Utils;

namespace Core.Transactions;

[Serializable]
public class Input
{
    public Input(string previousTransactionHash, int outputIndex, string publicKey)
    {
        PreviousTransactionHash = previousTransactionHash;
        OutputIndex = outputIndex;
        PublicKey = publicKey;
    }
    
    public string PreviousTransactionHash { get; }
    
    public string Hash => Hashing
        .SumSha256(PreviousTransactionHash, OutputIndex.ToString(), PublicKey, Signature ?? string.Empty)
        .ToHexDigest();
    
    public int OutputIndex { get; }
    
    public string Signature { get; set; }

    public string PublicKey { get; set; }

    public bool BelongsTo(string publicKeyHash) => RsaUtils.HashPublicKey(PublicKey).ToHexDigest() == publicKeyHash;
}