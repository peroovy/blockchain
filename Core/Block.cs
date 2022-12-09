namespace Core;

public class Block
{
    public Block(
        string previousHash, string hash,
        long timestamp, string data, int difficult, long nonce)
    {
        PreviousHash = previousHash;
        Hash = hash;
        Timestamp = timestamp;
        Data = data;
        Difficult = difficult;
        Nonce = nonce;
    }
    
    public string PreviousHash { get; }
    
    public string Hash { get; }

    public long Timestamp { get; }
    
    public string Data { get; }
    
    public int Difficult { get; }
    
    public long Nonce { get; }
}