using System;

namespace Core;

[Serializable]
public class SerializedBlock
{
    public SerializedBlock(
        string previousBlockHash, int height, string hash, long timestamp, string merkleRoot, int difficult, long nonce)
    {
        PreviousBlockHash = previousBlockHash;
        Height = height;
        Hash = hash;
        Timestamp = timestamp;
        MerkleRoot = merkleRoot;
        Difficult = difficult;
        Nonce = nonce;
    }

    public string PreviousBlockHash { get; set; }
    
    public int Height { get; set; }
    
    public string Hash { get; set; }

    public long Timestamp { get; set; }
    
    public string MerkleRoot { get; set; }
    
    public int Difficult { get; set; }
    
    public long Nonce { get; set; }
}